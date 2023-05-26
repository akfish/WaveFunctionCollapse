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
        foreach (SampleBase sample in samples.Items) {
            Model model;
            Console.WriteLine($"< {sample.name}");
            if (sample is Overlapping) {
                Overlapping o = (Overlapping)sample;
                model = new OverlappingModel(o.name, o.N, o.size, o.size, o.periodicInput, o.periodic, o.symmetry, o.ground, o.heuristic);
            } else if (sample is SimpleTiled) {
                SimpleTiled s = (SimpleTiled)sample;
                model = new SimpleTiledModel(s.name, s.subset, s.size, s.size, s.periodic, s.blackBackground, s.heuristic);
            } else {
                Console.WriteLine("Unknown sample type");
                continue;
            }
            for (int i = 0; i < sample.screenshots; i++)
            {
                for (int k = 0; k < 10; k++)
                {
                    Console.Write("> ");
                    int seed = random.Next();
                    bool success = model.Run(seed, sample.limit);
                    if (success)
                    {
                        Console.WriteLine("DONE");
                        model.Save($"output/{sample.name} {seed}.png");
                        if (model is SimpleTiledModel stmodel && ((SimpleTiled)sample).textOutput )
                            System.IO.File.WriteAllText($"output/{sample.name} {seed}.txt", stmodel.TextOutput());
                        break;
                    }
                    else Console.WriteLine("CONTRADICTION");
                }
            }
        }

        Console.WriteLine($"time = {sw.ElapsedMilliseconds}");
    }
}
