// Copyright (C) 2016 Maxim Gumin, The MIT License (MIT)

using System;
using System.Xml.Linq;
using System.Diagnostics;
using Schema;
using System.Xml.Serialization;

static class Program
{
    static void Main()
    {
        Stopwatch sw = Stopwatch.StartNew();
        var folder = System.IO.Directory.CreateDirectory("output");

        foreach (var file in folder.GetFiles()) file.Delete();

        XmlSerializer serializer = new XmlSerializer(typeof(Samples));
        Samples samples = (Samples)serializer.Deserialize(System.IO.File.OpenRead("samples.xml"));

        Random random = new();

        // For each sample in the XML file
        foreach (Object sample in samples.Items) {
            Model model;
            string name;
            int screenshots = 0;
            int limit = -1;
            if (sample is Overlapping) {
                Overlapping o = (Overlapping)sample;
                name = o.name;
                screenshots = o.screenshots;
                limit = o.limit;
                model = new OverlappingModel(o.name, o.N, o.size, o.size, o.periodicInput, o.periodic, o.symmetry, o.ground, o.heuristic);
            } else if (sample is SimpleTiled) {
                SimpleTiled s = (SimpleTiled)sample;
                name = s.name;
                screenshots = s.screenshots;
                limit = s.limit;
                model = new SimpleTiledModel(s.name, s.subset, s.size, s.size, s.periodic, s.blackBackground, s.heuristic);
            } else {
                Console.WriteLine("Unknown sample type");
                continue;
            }
            Console.WriteLine($"< {name}");
            for (int i = 0; i < screenshots; i++)
            {
                for (int k = 0; k < 10; k++)
                {
                    Console.Write("> ");
                    int seed = random.Next();
                    bool success = model.Run(seed, limit);
                    if (success)
                    {
                        Console.WriteLine("DONE");
                        model.Save($"output/{name} {seed}.png");
                        if (model is SimpleTiledModel stmodel && ((SimpleTiled)sample).textOutput )
                            System.IO.File.WriteAllText($"output/{name} {seed}.txt", stmodel.TextOutput());
                        break;
                    }
                    else Console.WriteLine("CONTRADICTION");
                }
            }
        }

        Console.WriteLine($"time = {sw.ElapsedMilliseconds}");
    }
}
