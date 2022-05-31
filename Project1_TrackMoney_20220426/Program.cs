using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using static TrackMoney.Menus;
using static TrackMoney.Helpers;

namespace TrackMoney
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "TrackMoney";
            Console.WindowWidth = 120;
            Console.WindowHeight = 40;
            LoadData(dataPath);
            ShowState(mainMenu, subheading: $"Your current balance is: { FormatN(GetItemsTotal()) }");
        }

        public static string dataPath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, @"data.json");

        public static List<Item> items = new List<Item>();

        // Display a list of the items, filtered and sorted, and shows potential selection
        public static void PrintItems(string orderBy = "", bool descending = true, string toShow = "all", int width = 64, int selected = -1, 
            string highlightColor = "", string highlightTextColor = "")
        {
            Panel("top", vPadding: 1, width: width);
            Panel("row", "Type".PadRight(14) + "Month".PadRight(16) + "Title".PadRight(24) + "Amount", width: width);
            Panel("hr", width: width);

            List<Item> filteredItems = new List<Item>();
            switch (toShow)
            {
                case "expenses":
                    filteredItems = items.Where(item => item.Amount < 0).ToList();
                    break;
                case "incomes":
                    filteredItems = items.Where(item => item.Amount > 0).ToList();
                    break;
                default:
                    filteredItems = items;
                    break;
            }

            List<Item> orderedItems = new List<Item>();
            if (orderBy != "")
            {
                orderBy = FirstLetterToUpper(orderBy);
                orderedItems = descending ?
                                filteredItems.OrderByDescending(item => item.GetType().GetProperty(orderBy).GetValue(item)).ToList() :
                                filteredItems.OrderBy(item => item.GetType().GetProperty(orderBy).GetValue(item)).ToList();
            }
            else orderedItems = filteredItems;

            for (int i = 0; i < orderedItems.Count; i++)
            {
                Item item = orderedItems[i];    
                string type = item.Amount > 0 ? "Income" : "Expense";
                bool isHighlighted = i == selected - 1;
                Panel("row",
                    type.PadRight(14) +
                    item.Date.ToString("MMMM yyyy", ci).PadRight(16) +
                    item.Title.PadRight(24) +
                    TextAlign(FormatN(item.Amount), 10, "right"),
                    width: width,
                    highlight: isHighlighted,
                    highlightColor: isHighlighted && highlightColor is not null ? highlightColor : "",
                    highlightTextColor: isHighlighted && highlightTextColor is not null  ? highlightTextColor : ""
                );
            }
            Panel("hr", width: width);
            Panel("row", TextAlign("Balance: " + TextAlign(FormatN(GetItemsTotal()), 13, "right"), width, "right"), width: width);
            Panel("bottom", vPadding: 1, width: width);
        }

        public static void AddNew(string typeOfItem = "expense")
        {
            Console.CursorVisible = true;
            int hMargin = (Console.WindowWidth / 2) - (48 / 2);
            Console.WriteLine();
            string input = "";
            string title = "";
            int amount, month = 0;
            DateTime date = new DateTime();
            bool isNumber, isDate = false;
            do
            {
                Console.Write("".PadRight(hMargin) + "Enter " + typeOfItem + " amount: ");
                input = Console.ReadLine();
                if (wantsToQuit(input)) ShowState(addNewMenu, 1, "Add New Expense / Income");
                isNumber = int.TryParse(input, out amount);
            }
            while (!isNumber);
            if (typeOfItem == "expense") amount = -Math.Abs(amount); // make sure it's negative if it's an expense

            do
            {
                Console.Write("".PadRight(hMargin) + "Enter " + typeOfItem + " title: ");
                input = Console.ReadLine();
                if (wantsToQuit(input)) ShowState(addNewMenu, 1, "Add New Expense / Income");
            }
            while (String.IsNullOrEmpty(input.Trim()));
            title = input;
            Console.WriteLine("".PadRight(hMargin) + "Enter date of " + typeOfItem);

            do
            {
                Console.Write("".PadRight(hMargin + 2) + "Month (1-12): ");
                input = Console.ReadLine();
                if (wantsToQuit(input)) ShowState(addNewMenu, 1, "Add New Expense / Income");
                int.TryParse(input, out month);
            }
            while (month < 1 || month > 12);

            do
            {
                Console.Write("".PadRight(hMargin + 2) + "Year: ");
                input = Console.ReadLine();
                if (wantsToQuit(input)) ShowState(addNewMenu, 1, "Add New Expense / Income");
                isDate = DateTime.TryParse(string.Format("{1}/{0}/1", month, input), out date);
            }
            while (!isDate);

            items.Add(new Item(date, title, amount));
            SaveData(dataPath);
            Console.WriteLine("\n".PadRight(hMargin) + $"New { typeOfItem } added.");

            Console.Write("\n".PadRight(hMargin) + $"Would you like to add a new { typeOfItem }? (Y / N) ");
            bool validInput = false;
            while (!validInput)
            {
                ConsoleKeyInfo keyPressed = Console.ReadKey(true);
                switch (keyPressed.Key)
                {
                    case ConsoleKey.Y:
                        ShowState(listMenuSortBy, 1, "Add New Income", current: () => AddNew("income"));
                        break;
                    case ConsoleKey.N:
                    case ConsoleKey.Enter:
                    case ConsoleKey.Escape:
                        ShowState(addNewMenu, 1, "Add New Expense / Income");
                        break;
                    default:
                        break;
                }
            }
        }

        public static void EditItems(int selected = 1, bool editing = false, bool deleting = false)
        {
            Console.CursorVisible = false;
            if (selected <= 0) selected = items.Count;
            else if (selected > items.Count) selected = 1;
            string highlightColor = deleting ? "DarkRed" : "DarkYellow";
            string highlightTextColor = deleting ? "White" : "Black";
            int hMargin = (Console.WindowWidth / 2) - (48 / 2);
            PrintItems(selected: selected, highlightColor: highlightColor, highlightTextColor: highlightTextColor);

            // Editing
            if (editing)
            {
                Console.CursorVisible = true;
                Console.WriteLine();
                string input = "";
                int amount, month = 0;
                DateTime date = new DateTime();
                bool isNumber, isDate = false;

                static void BackToEditMenu(int selected)
                {
                    ShowState(subheading: "Edit Item  (edit, remove)", current: () => EditItems(selected));
                }

                // Edit amount
                Console.WriteLine("".PadRight(hMargin) + $"Current amount is: { FormatN(items[selected - 1].Amount) } ");
                do
                {
                    Console.Write("".PadRight(hMargin) + "Enter new amount: ");
                    input = Console.ReadLine();
                    if (wantsToQuit(input)) BackToEditMenu(selected);
                    isNumber = int.TryParse(input, out amount);
                }
                while (!isNumber);
                items[selected - 1].Amount = amount;
                SaveData(dataPath);

                // Edit title
                Console.WriteLine("\n".PadRight(hMargin) + $"Current title is: { items[selected - 1].Title.Trim() } ");
                do
                {
                    Console.Write("".PadRight(hMargin) + "Enter new title: ");
                    input = Console.ReadLine();
                    if (wantsToQuit(input)) BackToEditMenu(selected);
                }
                while (String.IsNullOrEmpty(input.Trim()));
                items[selected - 1].Title = input;
                SaveData(dataPath);

                // Edit date
                Console.WriteLine("\n".PadRight(hMargin) + $"Current date is: { items[selected - 1].Date.ToString("MM / yyyy") } ");
                Console.WriteLine("".PadRight(hMargin) + "Enter new date: ");
                
                // Edit month
                do
                {
                    Console.Write("".PadRight(hMargin + 2) + "Month (1-12): ");
                    input = Console.ReadLine();
                    if (wantsToQuit(input)) BackToEditMenu(selected);
                    int.TryParse(input, out month);
                }
                while (month < 1 || month > 12);
                
                // Edit year
                do
                {
                    Console.Write("".PadRight(hMargin + 2) + "Year: ");
                    input = Console.ReadLine();
                    if (wantsToQuit(input)) ShowState(addNewMenu, 1, "Add New Expense / Income");
                    isDate = DateTime.TryParse(string.Format("{1}/{0}/1", month, input), out date);
                }
                while (!isDate);
                items[selected - 1].Date = date;

                SaveData(dataPath);
                BackToEditMenu(selected);
            }

            // Deleting
            else if (deleting)
            {
                Console.CursorVisible = true;
                Console.Write(TextAlign("Are you sure you would like to delete this item? (Y / N) ", Console.WindowWidth, "center"));
                bool validInput = false;
                while (!validInput)
                {
                    ConsoleKeyInfo keyPressed = Console.ReadKey(true);
                    switch (keyPressed.Key)
                    {
                        case ConsoleKey.Y or ConsoleKey.Enter:
                            items.RemoveAt(selected - 1);
                            SaveData(dataPath);
                            ShowState(subheading: "Edit Item  (edit, remove)", current: () => EditItems(selected));
                            break;
                        case ConsoleKey.N or ConsoleKey.Escape:
                            ShowState(subheading: "Edit Item  (edit, remove)", current: () => EditItems(selected));
                            break;
                        default:
                            break;
                    }
                }
            }

            // Selector function for editing-deleting
            else
            {
                Console.WriteLine("\n" + TextAlign("Use the Arrow Keys to select an item.", Console.WindowWidth, "center"));
                Console.WriteLine("\n" + TextAlign("Press Enter to edit.", Console.WindowWidth, "center"));
                Console.WriteLine("\n" + TextAlign("Press Delete or Backspace to remove.", Console.WindowWidth, "center"));
                Console.WriteLine("\n" + TextAlign("Press Esc or Q to return to the main menu", Console.WindowWidth, "center"));

                ConsoleKeyInfo keyPressed = Console.ReadKey(true);
                string currentItemInfo = $"{ items[selected - 1].Title } - { FormatN(items[selected - 1].Amount) }";
                switch (keyPressed.Key)
                {
                    case ConsoleKey.UpArrow or ConsoleKey.LeftArrow: // selection up
                        ShowState(subheading: "Edit Item  (edit, remove)", current: () => EditItems(selected - 1));
                        break;
                    case ConsoleKey.DownArrow or ConsoleKey.RightArrow or ConsoleKey.Tab: // selection down
                        ShowState(subheading: "Edit Item  (edit, remove)", current: () => EditItems(selected + 1));
                        break;
                    case ConsoleKey.Enter: // editing
                        ShowState(subheading: $"Edit Item \"{ currentItemInfo }\"", current: () => EditItems(selected, editing: true));
                        break;
                    case ConsoleKey.Delete or ConsoleKey.Backspace: // deleting
                        ShowState(subheading: $"Delete Item \"{ currentItemInfo }\"", current: () => EditItems(selected, deleting: true));
                        break;
                    case ConsoleKey.Q or ConsoleKey.Escape: // quitting
                        ShowState(mainMenu);
                        break;
                    default:
                        break;
                }
            }
        }

        // Displays a menu UI for the options listed in the specified "menu" List
        public static void ShowMenu(List<MenuFunction> menu, int selected = 1, string subheading = "", int width = 45, Action current = null)
        {
            Panel("top", width: width, vPadding: 1);
            if (selected < 1) selected = menu.Count;
            else if (selected > menu.Count) selected = 1;
            for (int i = 0; i < menu.Count; i++)
            {
                Panel("row", "[" + (i + 1) + "] " + menu[i].Description, highlight: i == selected - 1, width: width);
            }
            Panel("bottom", width: width, vPadding: 1);
            Console.CursorVisible = false;
            SelectMenu(menu, selected, subheading, current: current);
        }

        // Redraws the menu onto the console to give the illusion of up-down selection
        static void SelectMenu(List<MenuFunction> menu, int selected = 1, string subheading = "", Action current = null)
        {
            ConsoleKeyInfo keyPressed = Console.ReadKey(true);
            switch (keyPressed.Key)
            {
                case ConsoleKey.Enter:
                    menu[selected - 1].Action.Invoke();
                    break;
                case ConsoleKey.UpArrow or ConsoleKey.LeftArrow or ConsoleKey.Backspace:
                    ShowState(menu, selected - 1, subheading, current: current);
                    break;
                case ConsoleKey.DownArrow or ConsoleKey.RightArrow or ConsoleKey.Tab:
                    ShowState(menu, selected + 1, subheading, current: current);
                    break;
                default: // If the keyPressed is not arrows/Enter, then check which number it is
                    Int32 keyNumber;
                    if (Int32.TryParse(keyPressed.KeyChar.ToString(), out keyNumber) && keyNumber <= menu.Count)
                    {
                        menu[keyNumber - 1].Action.Invoke();
                    }
                    else ShowState(menu, selected, subheading, current: current);
                    break;
            }
        }

        // Quitting routine
        public static void Quit()
        {
            SaveData(dataPath);
            Panel("heading", "Thank you for using TrackMoney!", width: 40);
            Environment.Exit(0);
        }
    }

}