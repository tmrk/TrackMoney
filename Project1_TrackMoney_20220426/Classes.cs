using System;
using System.Globalization;

namespace TrackMoney
{
	public class Item
	{
        public Item(DateTime date, string title, long amount)
        {
            Date = date;
            Title = title;
            Amount = amount;
        }

        public DateTime Date { get; set; }

		public string Title { get; set; }
		
		public long Amount { get; set; }
	}

    public class Column
    {
        public Column(string name = "", int width = 1, string propertyName = "")
        {
            Name = name;
            Width = Math.Max(width, name.Length);
            PropertyName = propertyName.Length != 0 ? propertyName : new CultureInfo("en-UK").TextInfo.ToTitleCase(name).Replace(" ", "");
        }
        public string Name { get; set; }
        public int Width { get; set; }
        public string PropertyName { get; set; }
    }

    public class MenuFunction
    {
        public MenuFunction(string description, Action action)
        {
            Description = description;
            Action = action;
        }
        public string Description { get; set; }
        public Action Action { get; set; }
    }

}
