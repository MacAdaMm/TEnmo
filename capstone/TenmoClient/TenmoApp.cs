﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using TenmoClient.Helpers;
using TenmoClient.Models;
using TenmoClient.Services;
using System.Linq;

namespace TenmoClient
{
    public class TenmoApp
    {
        private readonly TenmoConsoleService console = new TenmoConsoleService();
        private readonly TenmoApiService tenmoApiService;

        public TenmoApp(string apiUrl)
        {
            tenmoApiService = new TenmoApiService(apiUrl);
        }

        public void Run()
        {
            bool keepGoing = true;
            while (keepGoing)
            {
                // The menu changes depending on whether the user is logged in or not
                if (tenmoApiService.IsLoggedIn)
                {
                    keepGoing = RunAuthenticated();
                }
                else // User is not yet logged in
                {
                    keepGoing = RunUnauthenticated();
                }
            }
        }

        private bool RunUnauthenticated()
        {
            console.PrintLoginMenu();
            int menuSelection = console.PromptForInteger("Please choose an option", 0, 2, 1);
            while (true)
            {
                if (menuSelection == 0)
                {
                    return false;   // Exit the main menu loop
                }

                if (menuSelection == 1)
                {
                    // Log in
                    Login();
                    return true;    // Keep the main menu loop going
                }

                if (menuSelection == 2)
                {
                    // Register a new user
                    Register();
                    return true;    // Keep the main menu loop going
                }
                console.PrintError("Invalid selection. Please choose an option.");
                console.Pause();
            }
        }

        private bool RunAuthenticated()
        {
            console.PrintMainMenu(tenmoApiService.Username);
            int menuSelection = console.PromptForInteger("Please choose an option", 0, 6);
            if (menuSelection == 0)
            {
                // Exit the loop
                return false;
            }

            if (menuSelection == 1)
            {
                // View logged in user balance
                console.Pause("Your current account balance is :" + tenmoApiService.getBalanceById(tenmoApiService.UserId).ToString("C"));
            }

            if (menuSelection == 2)
            {
                // View your past transfers
                var transfers = tenmoApiService.GetAllTransfersForUser();
                Console.WriteLine($"Transfers Count: {transfers.Count}");
                foreach(var transfer in transfers) // placeholder see README for desired result. (USE CASE: 5)
                {
                    Console.WriteLine($"Id: {transfer.Id}");
                    Console.WriteLine($"Type: {transfer.Type}");
                    Console.WriteLine($"From: {transfer.From}");
                    Console.WriteLine($"To: {transfer.To}");
                    Console.WriteLine($"Amount: {transfer.Amount}");
                    Console.WriteLine($"Id: {transfer.Status}");
                    Console.WriteLine("--------------------");
                }
                console.Pause();
            }

            if (menuSelection == 3) // View pending transfer
            {
               
              var  PendingTransfers = (from x in  tenmoApiService.GetAllTransfersForUser() where x.Status == "Pending"  select x).ToList<Transfer>() ;
                Console.WriteLine($"Transfers Count: {PendingTransfers.Count}");
                foreach (var item in PendingTransfers) // placeholder see README for desired result. (USE CASE: 5)
                {
                    Console.WriteLine($"Id: {item.Id}");
                    Console.WriteLine($"Type: {item.Type}");
                    Console.WriteLine($"From: {item.From}");
                    Console.WriteLine($"To: {item.To}");
                    Console.WriteLine($"Amount: {item.Amount}");
                    Console.WriteLine($"Status: {item.Status}");
                    Console.WriteLine("------------------------");
                }
               
            // CHOOSE THE TRANSFER
            enterTransferID:
                int transferId = console.PromptForInteger("Enter transfers ID ");
                if ((PendingTransfers.FirstOrDefault(x=>x.Id == transferId)) == null)
                {
                    console.PrintError("Please choose Id for the list");
                    goto enterTransferID;
                }
                var TransferToBechange = PendingTransfers.FirstOrDefault(x => x.Id == transferId);
                // CHANGE THE STATUS either approved it or rejected 
               console.PrintApproveOrReject();
                int actionToTake = console.PromptForInteger("Please chosse number", 0, 2, 0);

                if (actionToTake == 0)
                {
                    return true;
                }

               tenmoApiService.ChangeTransferStatus(TransferToBechange.Id, actionToTake);



                 // COMMIT THE CHANGES IN BALANCE
            }

            if (menuSelection == 4) // Send Money
            {
                PrintingUserList.PrintUsers( tenmoApiService.GetAllUsers());
                enterID:
                int userIdtoSendMonyTo =   console.PromptForInteger("Enter user ID to send to");
                if (TheChecker.AreEqual( userIdtoSendMonyTo , tenmoApiService.UserId))
                {
                    console.PrintError("You can't send money to your self");
                    goto enterID;
                }
                enterMoney:
                decimal AmountOfMoneytoBeSend = console.PromptForDecimal("Enter amount of money");
                if (TheChecker.LeftGreaterthe(0, AmountOfMoneytoBeSend))
                {
                    console.PrintError("You can't send negative money");
                    goto enterMoney;
                }
                //call   transfer send money 
                tenmoApiService.TransferPay(tenmoApiService.UserId, userIdtoSendMonyTo, AmountOfMoneytoBeSend,true);


                console.Pause();
            }

            if (menuSelection == 5) // Request money
            {
                PrintingUserList.PrintUsers(tenmoApiService.GetAllUsers());
            enterID2:
                int userIdtoToRequestMoneyFrom = console.PromptForInteger("Enter user ID to send to");
                if (TheChecker.AreEqual(userIdtoToRequestMoneyFrom, tenmoApiService.UserId))
                {
                    console.PrintError("You can't request money from your self");
                    goto enterID2;
                }
            enterMoney2:
                decimal AmountOfMoneytoBeRequested = console.PromptForDecimal("Enter amount of money");
                if (TheChecker.LeftGreaterthe(0, AmountOfMoneytoBeRequested))
                {
                    console.PrintError("You can't send negative money");
                    goto enterMoney2;
                }
                tenmoApiService.TransferPay(tenmoApiService.UserId, userIdtoToRequestMoneyFrom, AmountOfMoneytoBeRequested, false);
            }

            if (menuSelection == 6)
            {
                // Log out
                tenmoApiService.Logout();
                console.PrintSuccess("You are now logged out");
            }

            return true;    // Keep the main menu loop going
        }

        private void ChangeTransforStatus(Transfer transferToBechange, int actionToTake)
        {
            // 1 Aproved
            // 2 Reject

                       
        }

        private void Login()
        {
            LoginUser loginUser = console.PromptForLogin();
            if (loginUser == null)
            {
                return;
            }

            try
            {
                ApiUser user = tenmoApiService.Login(loginUser);
                if (user == null)
                {
                    console.PrintError("Login failed.");
                }
                else
                {
                    console.PrintSuccess("You are now logged in");
                }
            }
            catch (Exception)
            {
                console.PrintError("Login failed.");
            }
            console.Pause();
        }

        private void Register()
        {
            LoginUser registerUser = console.PromptForLogin();
            if (registerUser == null)
            {
                return;
            }
            try
            {
                bool isRegistered = tenmoApiService.Register(registerUser);
                if (isRegistered)
                {
                    console.PrintSuccess("Registration was successful. Please log in.");
                }
                else
                {
                    console.PrintError("Registration was unsuccessful.");
                }
            }
            catch (Exception)
            {
                console.PrintError("Registration was unsuccessful.");
            }
            console.Pause();
        }
    }




}
