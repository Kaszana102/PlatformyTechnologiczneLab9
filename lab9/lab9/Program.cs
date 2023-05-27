using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace lab9
{
    internal class Program
    {
        static void Main(string[] args)
        {
            List<Car> myCars = new List<Car>(){
            new Car("E250", new Engine(1.8, 204, "CGI"), 2009),
            new Car("E350", new Engine(3.5, 292, "CGI"), 2009),
            new Car("A6", new Engine(2.5, 187, "FSI"), 2012),
            new Car("A6", new Engine(2.8, 220, "FSI"), 2012),
            new Car("A6", new Engine(3.0, 295, "TFSI"), 2012),
            new Car("A6", new Engine(2.0, 175, "TDI"), 2011),
            new Car("A6", new Engine(3.0, 309, "TDI"), 2011),
            new Car("S6", new Engine(4.0, 414, "TFSI"), 2012),
            new Car("S8", new Engine(4.0, 513, "TFSI"), 2012)
            };

            //zad 1
            LINQ(myCars);

            //zad 2
            SeDeserialize(myCars);

            //zad 3
            XPath();

            //zad 4
            createXmlFromLinq(myCars);

            //zad 5
            LINQtoXML(myCars);

            //zad 6
            modyfikacja();
        }




        public static void LINQ(List<Car> myCars)
        {
            Console.WriteLine("linq1");
            var linq1 = myCars.Where(x => x.model.Equals("A6"))
                                .Select(x => new
                                {
                                    engineType = String.Compare(x.motor.model, "TDI") == 0
                                                                   ? "diesel"
                                                                   : "petrol",
                                    hppl = x.motor.horsePower / x.motor.displacement,
                                });
                                

            foreach (var elem in linq1)
            {
                Console.WriteLine(elem);
            }


            Console.WriteLine("\nlinq2");
            var linq2 = linq1.GroupBy(x => x.engineType)
                    .Select(x => new
                    {
                        hppl = x.Average(s => s.hppl).ToString(),
                    });


            foreach (var elem in linq2)
            {
                Console.WriteLine(elem);
            }
        }

        public static void SeDeserialize(List<Car> myCars)
        {
            Console.WriteLine("\nzad 2");
            var fileName = "CarsCollection.xml";
            var currentDirectory = Directory.GetCurrentDirectory();
            var filePath = Path.Combine(currentDirectory, fileName);


            //serializacja
            XmlSerializer serializer = new XmlSerializer(typeof(List<Car>),
                new XmlRootAttribute("cars"));
            using (TextWriter writer = new StreamWriter(fileName))
            {
                serializer.Serialize(writer, myCars);
            }

            //deserializacja:
            List<Car> deserializedList = new List<Car>();
            using (Stream reader = new FileStream(filePath, FileMode.Open))
            {
                deserializedList = (List<Car>)serializer.Deserialize(reader);
            }

            Console.WriteLine("Modele samochodów: ");
            foreach (var elem in deserializedList)
            {
                Console.WriteLine(elem.model); // <- coś popraw!-----------------------------------------------------
            }

        }

        public static void XPath()
        {

            Console.WriteLine("\nzad 3");
            XElement rootNode = XElement.Load("CarsCollection.xml");
            double avgHP = (double)rootNode.XPathEvaluate("sum(//car/engine[@model!=\"TDI\"]/horsePower) div count(//car/engine[@model!=\"TDI\"])");

            Console.WriteLine("avgHp = " + avgHP);


            string distinct = "//car/model[not(. = preceding:: model)]";
            IEnumerable<XElement> models = rootNode.XPathSelectElements(distinct);
            foreach (var model in models)
            {
                Console.WriteLine(model);
            }
        }

        private static void createXmlFromLinq(List<Car> myCars)
        {
            IEnumerable<XElement> nodes = myCars
                .Select(n =>
                new XElement("car",
                    new XElement("model", n.model),
                    new XElement("engine",
                        new XAttribute("model", n.motor.model),
                        new XElement("displacement", n.motor.displacement),
                        new XElement("horsePower", n.motor.horsePower)),
                    new XElement("year", n.year))); ;//LINQ query expressions
            XElement rootNode = new XElement("cars", nodes); //create a root node to contain the query results

            rootNode.Save("CarsFromLinq.xml");

        }

        public static void LINQtoXML(List<Car> myCars)
        {
            IEnumerable<XElement> rows = myCars
                .Select(car =>
                new XElement("tr", new XAttribute("style", "border: 2px solid black"),
                    new XElement("td", new XAttribute("style", "border: 2px double black"), car.model),
                    new XElement("td", new XAttribute("style", "border: 2px double black"), car.motor.model),
                    new XElement("td", new XAttribute("style", "border: 2px double black"), car.motor.displacement),
                    new XElement("td", new XAttribute("style", "border: 2px double black"), car.motor.horsePower),
                    new XElement("td", new XAttribute("style", "border: 2px double black"), car.year)));
            XElement table = new XElement("table", new XAttribute("style", "border: 2px double black"), rows);
            XElement cars = XElement.Load("cars.html");
            XElement calosc = cars.Element("{http://www.w3.org/1999/xhtml}body");
            calosc.Add(table);
            calosc.Save("carsWithTable.html");
        }

        public static void modyfikacja()
        {
            XElement original = XElement.Load("CarsCollection.xml");
            foreach (var car in original.Elements())
            {
                foreach (var field in car.Elements())
                {
                    if (field.Name == "engine")
                    {
                        foreach (var engineField in field.Elements())
                        {
                            if (engineField.Name == "horsePower")
                            {
                                engineField.Name = "hp";
                            }
                        }
                    }
                    else if (field.Name == "model")
                    {
                        var yearField = car.Element("year");
                        XAttribute attribute = new XAttribute("year", yearField.Value);
                        field.Add(attribute);
                        yearField.Remove();
                    }
                }
            }
            original.Save("CarsCollectionZmodyfikowany.xml");
        }


    }
    [XmlType("car")]
    public class Car
    {
        public string model { get; set; }
        public int year { get; set; }
        [XmlElement(ElementName = "engine")]
        public Engine motor { get; set; }

        public Car() { }
        public Car(string model,Engine motor, int year)
        {
            this.model = model;
            this.year = year;
            this.motor = motor;
        }
    }


    [XmlRoot(ElementName = "engine")]
    public class Engine
    {
        public double displacement{ get; set; }
        public double horsePower { get; set; }
        [XmlAttribute]
        public string model{ get; set; }

        public Engine() { }
        public Engine(double displacement,double horsePower, string model)
        {
            this.displacement = displacement;  
            this.horsePower = horsePower;   
            this.model = model;
        }
    }
}
