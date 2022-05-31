using System.Collections.Generic;
using static TrackMoney.Program;
using static TrackMoney.Helpers;

namespace TrackMoney
{
    public class Menus
    {
        // A list of the available menu items and the functions they trigger
        public static List<MenuFunction> mainMenu = new List<MenuFunction>
        {
            new MenuFunction("Show items (All / Expense(s) / Income(s))", ()=> ShowState(listMenu,   1, "Show Items",                current: () => PrintItems())),
            new MenuFunction("Add New Expense / Income",                  ()=> ShowState(addNewMenu, 1, "Add New Expense / Income")),
            new MenuFunction("Edit Item (edit, remove)",                  ()=> ShowState(subheading: "Edit Item  (edit, remove)", current: () => EditItems())),
            new MenuFunction("Save and Quit",                             ()=> Quit())
        };

        public static List<MenuFunction> listMenu = new List<MenuFunction>
        {
            new MenuFunction("Sort list by columns",        ()=> ShowState(listMenuSortBy, 1, "Sorted by month (Newest first)", current: () => PrintItems("date"))),
            new MenuFunction("Show all items",              ()=> ShowState(listMenu,       2, "Show All Items",                 current: () => PrintItems())),
            new MenuFunction("Show expenses only",          ()=> ShowState(listMenu,       3, "Show Expenses",                  current: () => PrintItems(toShow: "expenses"))),
            new MenuFunction("Show incomes only",           ()=> ShowState(listMenu,       4, "Show Incomes",                   current: () => PrintItems(toShow: "incomes"))),
            new MenuFunction("<< Go back to the main menu", ()=> ShowState(mainMenu))
        };

        // Sort items in ascending or descending order, by month, amount or title
        public static List<MenuFunction> listMenuSortBy = new List<MenuFunction>
        {
            new MenuFunction("Sort by month (Newest first)",   ()=> ShowState(listMenuSortBy, 1, "Sorted by month (Newest first)",   current: () => PrintItems("date"))),
            new MenuFunction("Sort by month (Oldest first)",   ()=> ShowState(listMenuSortBy, 2, "Sorted by month (Oldest first)",   current: () => PrintItems("date", false))),
            new MenuFunction("Sort by amount (Highest first)", ()=> ShowState(listMenuSortBy, 3, "Sorted by amount (Highest first)", current: () => PrintItems("amount"))),
            new MenuFunction("Sort by amount (Lowest first)",  ()=> ShowState(listMenuSortBy, 4, "Sorted by amount (Lowest first)",  current: () => PrintItems("amount", false))),
            new MenuFunction("Sort by title (Ascending)",      ()=> ShowState(listMenuSortBy, 5, "Sorted by title (A => Z)",         current: () => PrintItems("title", false))),
            new MenuFunction("Sort by title (Descending)",     ()=> ShowState(listMenuSortBy, 6, "Sorted by title (Z => A)",         current: () => PrintItems("title"))),
            new MenuFunction("<< Go back to menu",             ()=> ShowState(listMenu,       1, "Show All Items",                   current: () => PrintItems()))
        };

        public static List<MenuFunction> addNewMenu = new List<MenuFunction>
        {
            new MenuFunction("Add a new income",            ()=> ShowState(listMenuSortBy, 1, "Add New Income",  current: () => AddNew("income"))),
            new MenuFunction("Add a new expense",           ()=> ShowState(listMenuSortBy, 1, "Add New Expense", current: () => AddNew("expense"))),
            new MenuFunction("<< Go back to the main menu", ()=> ShowState(mainMenu))
        };
    }
}
