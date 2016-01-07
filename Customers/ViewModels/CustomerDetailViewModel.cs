﻿using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using System.Linq;

namespace Customers
{
    public class CustomerDetailViewModel : BaseViewModel
    {
        bool _IsNewCustomer;

        readonly Geocoder _Geocoder;

        public CustomerDetailViewModel(Customer account = null)
        {
            _Geocoder = new Geocoder();

            if (account == null)
            {
                _IsNewCustomer = true;
                Account = new Customer();
            }
            else
            {
                _IsNewCustomer = false;
                Account = account;
            }
        }

        public bool IsExistingCustomer { get { return !_IsNewCustomer; } }

        public string Title { get { return _IsNewCustomer ? "New Customer" : _Account.DisplayLastNameFirst; }  }

        Customer _Account;

        public Customer Account
        {
            get { return _Account; }
            set
            {
                _Account = value;
                OnPropertyChanged("Account");
            }
        }

        Position _Position;

        public Position Position
        {
            get { return _Position; }
            set
            {
                _Position = value;
                OnPropertyChanged("Position");
            }
        }

        Command _SaveCustomerCommand;

        /// <summary>
        /// Command to save customer
        /// </summary>
        public Command SaveCustomerCommand
        {
            get
            {
                return _SaveCustomerCommand ??
                    (_SaveCustomerCommand = new Command(async () =>
                        await ExecuteSaveCustomerCommand()));
            }
        }

        async Task ExecuteSaveCustomerCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            if (String.IsNullOrWhiteSpace(Account.LastName) || String.IsNullOrWhiteSpace(Account.FirstName))
            {
                // display an alert, letting the user know that we require a first and last name in order to save a new customer
                await Page.DisplayAlert(
                    title: "Invald name!", 
                    message: "A customer must have both a first and last name.",
                    cancel: "OK");
            }
            else
            {
                // send a message via MessaginCenter that we want the given customer to be saved
                MessagingCenter.Send(this.Account, "SaveCustomer");

                // perform a pop in order to navigate back to the customer list
                await Navigation.PopAsync();
            }

            IsBusy = false;
        }

        Command _EditCustomerCommand;

        /// <summary>
        /// Command to edit customer
        /// </summary>
        public Command EditCustomerCommand
        {
            get
            {
                return _EditCustomerCommand ??
                    (_EditCustomerCommand = new Command(async () =>
                        await ExecuteEditCustomerCommand()));
            }
        }

        async Task ExecuteEditCustomerCommand()
        {
            var editPage = new CustomerEditPage();

            var viewmodel = this;

            viewmodel.Page = editPage;

            editPage.BindingContext = viewmodel;

            await Navigation.PushAsync(editPage);
        }


        Command _DeleteCustomerCommand;

        /// <summary>
        /// Command to delete customer
        /// </summary>
        public Command DeleteCustomerCommand
        {
            get
            {
                return _DeleteCustomerCommand ??
                    (_DeleteCustomerCommand = new Command(async () =>
                        await ExecuteDeleteCustomerCommand()));
            }
        }

        async Task ExecuteDeleteCustomerCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            // display an alert and get the result of the user's decision
            var confirmDelete = 
                await Page.DisplayAlert(
                    title: String.Format("Delete {0}?", Account.DisplayName), 
                    message: null, 
                    accept: "Delete", 
                    cancel: "Cancel");

            if (confirmDelete)
            {
                // send a message via MessaginCenter that we want the given customer to be deleted
                MessagingCenter.Send(this.Account, "DeleteCustomer");

                // perform a pop in order to navigate back to the customer list
                await Navigation.PopAsync();
            }

            IsBusy = false;
        }

        public async Task LoadPins()
        {
            var positions = await _Geocoder.GetPositionsForAddressAsync(Account.AddressString);

            var enumerablePositions = positions as Position[] ?? positions.ToArray();

            if (positions != null && enumerablePositions.Any())
            {
                Position = enumerablePositions.First();
            }
        }
    }
}

