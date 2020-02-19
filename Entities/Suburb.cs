using HtmlAgilityPack;
using System;
using System.Collections.Generic;

namespace PropertyScraper.Entities
{
    public class Suburb
    {
        public string Name { get; set; }
        public int Count { get; set; }
        

        public override string ToString()
        {
            return $"Suburb: Name:{Name}, Count:{Count}";
        }

        public static Suburb AddUpdate(Dictionary<string, Suburb> list, string id)
        {
            Suburb item = null;

            if (!list.ContainsKey(id))
            {
                item = new Suburb
                {
                    Name = id,
                    Count = 1,
                };

                list.Add(id, item);

                Console.WriteLine("CREATING " + item.ToString());
            }
            else
            {
                item = list[id];
                item.Count += 1;

                Console.WriteLine("UPDATING " + item.ToString());
            }

            return item;
        }
    }
}
