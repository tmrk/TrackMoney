using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using static TrackMoney.Program;

namespace TrackMoney
{
    public class Helpers
    {
        public static CultureInfo ci = new CultureInfo("en-UK");

        // Draw the current state onto the console
        public static void ShowState(List<MenuFunction> menu = null, int menuSelected = 1, string subheading = "",
            bool filterTable = false, string orderBy = "", string thenBy = "", Action current = null)
        {
            Console.Clear();
            PrintHeader(subheading: subheading);
            if (current is not null) current.Invoke();
            if (menu is not null) ShowMenu(menu, menuSelected, subheading, current: current);
        }

        // Generates a panel window and draws it onto the consol to show output in a nice way
        public static void Panel(string partToPrint, string content = "",
            int width = 0, int hMargin = 0, int tMargin = 0, int bMargin = 0, int hPadding = 2, int vPadding = 0, int border = 1, int colspan = 1,
            string textAlign = "", string color = "", string fontColor = "",
            bool highlight = false, string highlightColor = "", string highlightTextColor = "",
            string subheading = "", List<Column> cols = null, List<Item> rows = null, bool filterTable = false)
        {
            if (width == 0) width = Console.WindowWidth - (hMargin * 2) - (hPadding * 2) - 2; // sets the panel to full window width if no width is defined
            if (hMargin == 0) hMargin = ((Console.WindowWidth - hMargin - width - hPadding) / 2) - (border * 2); // centers the panel if no hMargin is defined

            content = TextAlign(content, width, textAlign);
            ConsoleColor panelColor, textColor, panelColorHighlight, textColorHighlight;
            if (Enum.TryParse(color, out panelColor)) { }
            else panelColor = ConsoleColor.Gray;
            if (Enum.TryParse(fontColor, out textColor)) { }
            else textColor = ConsoleColor.Black;
            if (highlightColor != "" || highlightTextColor != "") highlight = true;
            if (highlight)
            {
                if (Enum.TryParse(highlightColor, out panelColorHighlight)) { }
                else panelColorHighlight = ConsoleColor.DarkGreen;
                if (Enum.TryParse(highlightTextColor, out textColorHighlight)) { }
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

        // Display a title at the top, incl. subtitle
        public static void PrintHeader(string subheading = "")
        {
            Console.Clear();
            Panel("heading", "TrackMoney v1.0", subheading: subheading, width: 45, tMargin: 1);
        }

        public static bool wantsToQuit(string userInput)
        {
            return (userInput.ToLower().Trim() == "exit" ||
                    userInput.ToLower().Trim() == "q" ||
                    userInput.ToLower().Trim() == "quit");
        }

        // Returns a string that aligns the passed text inside the width of a containing box
        public static string TextAlign(string text = "", int boxLength = 1, string textAlign = "")
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
        public static string FormatN(double number, int decimals = 0)
        {
            return number.ToString("N" + decimals, CultureInfo.InvariantCulture);
        }

        public static long GetItemsTotal()
        {
            long sum = 0;
            for (int i = 0; i < items.Count; i++) sum += items[i].Amount;
            return sum;
        }

        public static string FirstLetterToUpper(string str)
        {
            if (str == null) return null;
            if (str.Length > 1) return char.ToUpper(str[0]) + str.Substring(1).ToLower();
            return str.ToUpper();
        }

        public static void LoadData(string path)
        {
            if (File.Exists(path) && File.ReadAllText(path).Trim() != "")
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

        public static void SaveData(string path)
        {
            File.WriteAllText(path, JsonSerializer.Serialize(items));
        }
    }
}
