using System;
using System.Collections.Generic;
using System.Text;

namespace CoffSip.Application.MenuItems
{
    // Signals that an authenticated operator does not own the requested item.
    public sealed class MenuItemOwnershipException : Exception
    {
        public MenuItemOwnershipException()
            : base("The operator does not own this menu item.")
        {
        }
    }
}
