using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Globalization;

namespace TrackMoney
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "TrackMoney";

            LoadData(dataPath);

            //ShowMenu();

            ShowState(mainMenu);

            //Console.WriteLine(JsonSerializer.Serialize(items, new JsonSerializerOptions() { WriteIndented = true }));

        }

        static string dataPath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, @"data.json");

        static CultureInfo ci = new CultureInfo("en-UK");

        static List<Item> items = new List<Item>();

        static void LoadData(string path)
        {
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                items = JsonSerializer.Deserialize<List<Item>>(json);
            }
            else
            {
                items.Add(new Item(DateTime.Now, "Salary " + DateTime.Now.ToString("MMM", ci), 25000));
                items.Add(new Item(DateTime.Now.AddMonths(-1), "Salary " + DateTime.Now.AddMonths(-1).ToString("MMM", ci), 25000));
                items.Add(new Item(DateTime.Now.AddMonths(-2), "Salary " + DateTime.Now.AddMonths(-2).ToString("MMM", ci), 25000));
                items.Add(new Item(DateTime.Now.AddMonths(-3), "Salary " + DateTime.Now.AddMonths(-3).ToString("MMM", ci), 25000));
                items.Add(new Item(DateTime.Now.AddMonths(-1), "Rent", -8000));
                items.Add(new Item(DateTime.Now.AddMonths(-2), "Loan", 15000));
                items.Add(new Item(DateTime.Now.AddMonths(-2), "Shopping", -2345));
                items.Add(new Item(DateTime.Now.AddMonths(-2), "New clothes", -3200));
                items.Add(new Item(DateTime.Now.AddMonths(-2), "Bicycle", -1500));
                SaveData(dataPath);
            }
        }

        static void SaveData(string path)
        {
            File.WriteAllText(path, JsonSerializer.Serialize(items));
        }

        // A list of the available menu items and the functions they trigger
        public static List<MenuFunction> mainMenu = new List<MenuFunction>
        {
            new MenuFunction("Show items (All / Expense(s) / Income(s))", ()=> ShowItems()),
            new MenuFunction("Add New Expense / Income", ()=> AddNew()),
            new MenuFunction("Edit Item (edit, remove)", ()=> EditItem()),
            new MenuFunction("Save and Quit", ()=> Quit())
        };

        public static List<MenuFunction> listMenu = new List<MenuFunction>
        {
            new MenuFunction("Show all items", ()=> Quit()),
            new MenuFunction("Show expenses only", ()=> Quit()),
            new MenuFunction("Show incomes only", ()=> Quit())
            //new MenuFunction("Go back to the main menu", ()=> Quit())
        };

        static void ShowItems(string orderBy = "date", bool descending = true)
        {
            PrintPanel("heading", heading: "Show items");
            PrintPanel("top", vPadding: 1);
            PrintPanel("row", "Type".PadRight(14) + "Month".PadRight(16) + "Title".PadRight(24) + "Amount");
            PrintPanel("hr");

            orderBy = FirstLetterToUpper(orderBy);
            List<Item> orderedItems = descending ?
                          items.OrderByDescending(item => item.GetType().GetProperty(orderBy).GetValue(item)).ToList() :
                          items.OrderBy(item => item.GetType().GetProperty(orderBy).GetValue(item)).ToList();

            foreach (Item item in orderedItems)
            {
                string type = item.Amount > 0 ? "Income" : "Expense";
                PrintPanel("row", 
                    type.PadRight(14) + 
                    item.Date.ToString("MMMM yyyy", ci).PadRight(16) + 
                    item.Title.PadRight(24) + 
                    FormatN(item.Amount)
                );
                //Console.Write(item.Date.ToString("MMMM", ci).PadRight(10) + item.Title.PadRight(20) + item.Amount);
            }
            PrintPanel("bottom", vPadding: 1);
            ShowMenu(listMenu);
            Console.ReadLine();
        }

        static void AddNew()
        {

        }

        static void EditItem()
        {

        }

        // Display a title at the top, incl. subtitle
        static void PrintHeader(string subheading = "")
        {
            Console.Clear();
            Panel("heading", "TrackMoney v1.0", subheading: subheading, width: 60, tMargin: 1);
        }

        static public void ShowState(List<MenuFunction> menu, int menuSelected = 1, string subheading = "", bool filterTable = false, string orderBy = "", string thenBy = "")
        {
            // Set or reset global options so that they stay the same between state changes
            //filterTableTemp = filterTable;
            //orderByTemp = orderBy;
            //thenByTemp = thenBy;

            // Draw the current state onto the console
            PrintHeader(subheading: subheading);
            //PrintAssets();
            ShowMenu(menu, menuSelected, subheading);
            SelectMenu(menu, menuSelected, subheading);
        }

        // Displays a menu UI for the options listed in the specified "menu" List
        static void ShowMenu(List<MenuFunction> menu, int selected = 1, string subheading = "", int width = 60)
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
            SelectMenu(menu, selected, subheading);
        }

        // Implements redrawing the menu onto the console to give the illusion of up-down selection
        static void SelectMenu(List<MenuFunction> menu, int selected = 1, string subheading = "")
        {
            ConsoleKeyInfo keyPressed = Console.ReadKey(true);
            switch (keyPressed.Key)
            {
                case ConsoleKey.Enter:
                    menu[selected - 1].Action.Invoke();
                    break;
                case ConsoleKey.UpArrow or ConsoleKey.LeftArrow or ConsoleKey.Backspace:
                    ShowState(menu, selected - 1, subheading);
                    break;
                case ConsoleKey.DownArrow or ConsoleKey.RightArrow or ConsoleKey.Tab:
                    ShowState(menu, selected + 1, subheading);
                    break;
                default: // If the keyPressed is not arrows/Enter, then check which number it is
                    Int32 keyNumber;
                    if (Int32.TryParse(keyPressed.KeyChar.ToString(), out keyNumber) && keyNumber <= menu.Count)
                    {
                        menu[keyNumber - 1].Action.Invoke();
                    }
                    else ShowState(menu, selected, subheading);
                    break;
            }
        }


        // Generates a panel window and draws it onto the consol to show output in a nice way
        static void Panel(string partToPrint, string content = "",
            int width = 0, int hMargin = 0, int tMargin = 0, int bMargin = 0, int hPadding = 2, int vPadding = 0, int border = 1, int colspan = 1,
            string textAlign = "", string color = "", string fontColor = "",
            bool highlight = false, string highlightColor = "", string highlightTextColor = "",
            string subheading = "", List<Column> cols = null, List<Item> rows = null, bool filterTable = false)
        {
            if (width == 0) width = Console.WindowWidth - (hMargin * 2) - (hPadding * 2) - 2; // sets the panel to full window width if no width is defined
            if (hMargin == 0) hMargin = ((Console.WindowWidth - hMargin - width - hPadding) / 2) - (border * 2); // centers the panel if no hMargin is defined

            content = TextAlign(content, width, textAlign);
            ConsoleColor panelColor, textColor, panelColorHighlight, textColorHighlight;
            if (Enum.TryParse(FirstLetterToUpper(color), out panelColor)) { }
            else panelColor = ConsoleColor.Gray;
            if (Enum.TryParse(FirstLetterToUpper(fontColor), out textColor)) { }
            else textColor = ConsoleColor.Black;
            if (highlightColor != "" || highlightTextColor != "") highlight = true;
            if (highlight)
            {
                if (Enum.TryParse(FirstLetterToUpper(highlightColor), out panelColorHighlight)) { }
                else panelColorHighlight = ConsoleColor.DarkGreen;
                if (Enum.TryParse(FirstLetterToUpper(highlightTextColor), out textColorHighlight)) { }
                else textColorHighlight = ConsoleColor.White;
            }
            else
            {
                panelColorHighlight = panelColor;
                textColorHighlight = textColor;
            }
            if (tMargin != 0) for (int i = 0; i < tMargin; i++) Console.WriteLine("");
            switch (partToPrint.ToLower())
            {
                case "top":
                    Console.ForegroundColor = panelColor;
                    Console.Write(new string(' ', hMargin));
                    if (border == 1) Console.WriteLine("┌" + new string('─', width + (hPadding * 2)) + "┐");
                    for (int i = 0; i < vPadding; i++) Panel("br", width: width, hMargin: hMargin, color: color, fontColor: fontColor);
                    break;
                case "bottom":
                    for (int i = 0; i < vPadding; i++) Panel("br", width: width, hMargin: hMargin, color: color, fontColor: fontColor);
                    Console.Write(new string(' ', hMargin));
                    Console.ForegroundColor = panelColor;
                    if (border == 1) Console.WriteLine("└" + new string('─', width + (hPadding * 2)) + "┘");
                    Console.ResetColor();
                    break;
                case "left":
                    Console.ResetColor();
                    Console.Write(new string(' ', hMargin));
                    if (border == 1)
                    {
                        Console.ForegroundColor = panelColor;
                        Console.Write("│");
                        Console.ResetColor();
                    }
                    if (highlight)
                    {
                        Console.BackgroundColor = panelColorHighlight;
                        Console.ForegroundColor = textColorHighlight;
                    }
                    else
                    {
                        Console.BackgroundColor = panelColor;
                        Console.ForegroundColor = textColor;
                    }
                    Console.Write(new string(' ', hPadding));
                    break;
                case "right":
                    Console.Write(new string(' ', Math.Max(width + hMargin + hPadding + 1 - Console.CursorLeft, 0)));
                    Console.Write(new string(' ', hPadding));
                    Console.ResetColor();
                    if (border == 1)
                    {
                        Console.ForegroundColor = panelColor;
                        Console.WriteLine("│");
                    }
                    else Console.WriteLine("");
                    break;
                case "row":
                    for (int i = 0; i < vPadding; i++) Panel("br", width: width, color: color, fontColor: fontColor);
                    Panel("left", width: width, border: border, color: color, fontColor: fontColor, highlight: highlight, highlightColor: highlightColor, highlightTextColor: highlightTextColor, hMargin: hMargin);
                    Console.Write(content);
                    Panel("right", width: width, border: border, color: color, fontColor: fontColor, hMargin: hMargin);
                    for (int i = 0; i < vPadding; i++) Panel("br", width: width, color: color, fontColor: fontColor);
                    break;
                case "hr":
                    Panel("left", width: width, hMargin: hMargin, color: color, fontColor: fontColor);
                    Console.Write(new string('─', width));
                    Panel("right", width: width, hMargin: hMargin, color: color, fontColor: fontColor);
                    break;
                case "br":
                    Panel("left", width: width, hMargin: hMargin, color: color, fontColor: fontColor);
                    Console.Write(" ");
                    Panel("right", width: width, hMargin: hMargin, color: color, fontColor: fontColor);
                    break;
                case "heading":
                    Panel("top", width: width, vPadding: 1, hMargin: hMargin, color: color, fontColor: fontColor);
                    Panel("row", content, textAlign: "center", width: width, hMargin: hMargin, color: color, fontColor: fontColor);
                    if (subheading != "")
                    {
                        Panel("br", color: color, width: width, hMargin: hMargin);
                        Panel("row", subheading, textAlign: "center", width: width, hMargin: hMargin, color: color, fontColor: fontColor);
                    }
                    Panel("bottom", width: width, vPadding: 1, hMargin: hMargin, color: color, fontColor: fontColor);
                    break;
                case "table":
                    string topRow = "";
                    int listWidth = 0;
                    for (int i = 0; i < cols.Count; i++)
                    {
                        Column col = cols[i];
                        int topRowColspan = colspan;
                        if (i == cols.Count - 1) topRowColspan = 0;
                        topRow += col.Name + new string(' ', col.Width - col.Name.Length + topRowColspan);
                        listWidth += col.Width + topRowColspan;
                    }
                    Panel("top", width: listWidth, vPadding: 1, color: color, fontColor: fontColor);
                    Panel("row", topRow, width: listWidth, color: color, fontColor: fontColor);
                    Panel("hr", width: listWidth, color: color, fontColor: fontColor);
                    foreach (var item in rows)
                    {
                        if (filterTable)
                        {
                            if (false)
                            {
                                highlightColor = "Red";
                                highlightTextColor = "";

                            }
                            else
                            {
                                highlightColor = "";
                                highlightTextColor = "";
                            }
                        }
                        //Panel("row", PrintAssetByColumns(item, cols, colspan: colspan), width: listWidth, color: color, fontColor: fontColor, filterTable: filterTable, highlightColor: highlightColor, highlightTextColor: highlightTextColor);
                    }
                    Panel("bottom", width: listWidth, vPadding: 1, color: color, fontColor: fontColor);
                    break;
            }
            if (bMargin != 0) for (int i = 0; i < bMargin; i++) Console.WriteLine("");
        }

        // Returns a string that aligns the passed text inside the width of a containing box
        static string TextAlign(string text = "", int boxLength = 1, string textAlign = "")
        {
            int leftPadding = 0;
            switch (textAlign.ToLower().Trim())
            {
                case "right" or "r":
                    leftPadding = boxLength - text.Length;
                    break;
                case "center" or "c":
                    leftPadding = (boxLength - text.Length) / 2;
                    break;
            }
            return new string(' ', Math.Max(leftPadding, 0)) + text;
        }

        // adds a thousands separator to numbers
        static string FormatN(double number, int decimals = 0)
        {
            return number.ToString("N" + decimals, CultureInfo.InvariantCulture);
        }

        // Generates a panel window and draws it onto the consol to show output in a nice way
        static void PrintPanel(string partToPrint, string content = "", int margin = 20, int hPadding = 4, int vPadding = 0, int border = 1, string textAlign = "", string panel = "", string text = "", bool highlight = false, string highlightColor = "", string highlightTextColor = "", string heading = "", string subheading = "")
        {
            int panelWidth = Console.WindowWidth - (margin * 2) - (hPadding * 2) - 2;
            content = TextAlign(content, panelWidth, textAlign);

            ConsoleColor panelColor, textColor, panelColorHighlight, textColorHighlight;
            if (Enum.TryParse(FirstLetterToUpper(panel), out panelColor)) {}
            else panelColor = ConsoleColor.Gray;
            if (Enum.TryParse(FirstLetterToUpper(text), out textColor)) {}
            else textColor = ConsoleColor.Black;
            if (highlightColor != "" || highlightTextColor != "") highlight = true;
            if (highlight)
            {
                if (Enum.TryParse(FirstLetterToUpper(highlightColor), out panelColorHighlight)) {}
                else panelColorHighlight = ConsoleColor.DarkGreen;
                if (Enum.TryParse(FirstLetterToUpper(highlightTextColor), out textColorHighlight)) {}
                else textColorHighlight = ConsoleColor.White;
            }
            else
            {
                panelColorHighlight = panelColor;
                textColorHighlight = textColor;
            }
            switch (partToPrint.ToLower())
            {
                case "top":
                    Console.ForegroundColor = panelColor;
                    Console.Write(new string(' ', margin));
                    if (border == 1) Console.WriteLine("┌" + new string('─', panelWidth + (hPadding * 2)) + "┐");
                    for (int i = 0; i < vPadding; i++) PrintPanel("br", margin: margin);
                    break;
                case "bottom":
                    for (int i = 0; i < vPadding; i++) PrintPanel("br", margin: margin);
                    Console.Write(new string(' ', margin));
                    Console.ForegroundColor = panelColor;
                    if (border == 1) Console.WriteLine("└" + new string('─', panelWidth + (hPadding * 2)) + "┘");
                    Console.ResetColor();
                    break;
                case "left":
                    Console.ResetColor();
                    Console.Write(new string(' ', margin));
                    if (border == 1)
                    {
                        Console.ForegroundColor = panelColor;
                        Console.Write("│");
                        Console.ResetColor();
                    }
                    if (highlight)
                    {
                        Console.BackgroundColor = panelColorHighlight;
                        Console.ForegroundColor = textColorHighlight;
                    }
                    else
                    {
                        Console.BackgroundColor = panelColor;
                        Console.ForegroundColor = textColor;
                    }
                    Console.Write(new string(' ', hPadding));
                    break;
                case "right":
                    Console.Write(new string(' ', Math.Max(panelWidth + margin + hPadding + 1 - Console.CursorLeft, 0)));
                    Console.Write(new string(' ', hPadding));
                    Console.ResetColor();
                    if (border == 1)
                    {
                        Console.ForegroundColor = panelColor;
                        Console.WriteLine("│");
                    }
                    else Console.WriteLine("");
                    break;
                case "row":
                    for (int i = 0; i < vPadding; i++) PrintPanel("br");
                    PrintPanel("left", border: border, highlight: highlight, highlightColor: highlightColor, highlightTextColor: highlightTextColor, margin: margin);
                    Console.Write(content);
                    PrintPanel("right", border: border, margin: margin);
                    for (int i = 0; i < vPadding; i++) PrintPanel("br");
                    break;
                case "hr":
                    PrintPanel("left", margin: margin);
                    Console.Write(new string('─', panelWidth));
                    PrintPanel("right", margin: margin);
                    break;
                case "br":
                    PrintPanel("left", margin: margin);
                    Console.Write(" ");
                    PrintPanel("right", margin: margin);
                    break;
                case "heading":
                    Console.WriteLine("");
                    PrintPanel("top", vPadding: 1, margin: margin);
                    PrintPanel("row", heading, textAlign: "center", margin: margin);
                    if (subheading != "")
                    {
                        PrintPanel("br");
                        PrintPanel("row", subheading, textAlign: "center", margin: margin);
                    }
                    PrintPanel("bottom", vPadding: 1, margin: margin);
                    break;
            }
        }

        static string FirstLetterToUpper(string str)
        {
            if (str == null) return null;
            if (str.Length > 1) return char.ToUpper(str[0]) + str.Substring(1).ToLower();
            return str.ToUpper();
        }

        // Quitting routine
        static void Quit()
        {
            SaveData(dataPath);
            Panel("heading", "Thank you for using TrackMoney!", width: 40);
            Environment.Exit(0);
        }
    }
}
