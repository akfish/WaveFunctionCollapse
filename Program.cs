// Copyright (C) 2016 Maxim Gumin, The MIT License (MIT)

using System;
using System.Diagnostics;
using Schema;
using System.Xml.Serialization;

static class Program
{
  static void Main(string[] names)
  {
    Stopwatch sw = Stopwatch.StartNew();
    var folder = System.IO.Directory.CreateDirectory("output");

    foreach (var file in folder.GetFiles()) file.Delete();

    XmlSerializer serializer = new XmlSerializer(typeof(Samples));
    Samples samples = (Samples)serializer.Deserialize(System.IO.File.OpenRead("samples.xml"));


    SampleBase[] samplesToRun = names.Length == 0
        ? samples.Items
        : Array.ConvertAll(names, name => Array.Find(samples.Items, sample => sample.name == name));

    if (samplesToRun.Length == 0)
    {
      Console.WriteLine("No samples found");
      return;
    }

    Console.WriteLine($"Found {samplesToRun.Length} / {samples.Items.Length} samples in {sw.ElapsedMilliseconds} ms");

    // For each sample in the XML file
    foreach (SampleBase sample in samplesToRun)
    {
      Model model;
      Console.WriteLine($"==========================================================");
      Console.WriteLine($"Sample:\t{sample.name}");
      Console.Write("Type:\t");
      sw.Restart();
      if (sample is Overlapping)
      {
        Overlapping o = (Overlapping)sample;
        model = new OverlappingModel(o.name, o.size, o.size, o.periodicInput, o.periodic, o.symmetry, o.ground, o.heuristic);
        Console.WriteLine("Overlapping");
      }
      else if (sample is SimpleTiled)
      {
        SimpleTiled s = (SimpleTiled)sample;
        model = new SimpleTiledModel(s.name, s.subset, s.size, s.size, s.periodic, s.blackBackground, s.heuristic);
        Console.WriteLine("SimpleTiled");
      }
      else
      {
        Console.WriteLine("Unknown (Skipping)");
        continue;
      }
      Console.WriteLine($"> Created in {sw.ElapsedMilliseconds}ms");
      // Use the same seed for each run
      Random random = new(42);
      for (int i = 0; i < sample.screenshots; i++)
      {
        // Update the seed for each screenshot
        int seed = random.Next();
        for (int k = 0; k < 10; k++)
        {
          Console.Write($"> Run {i + 1}/{sample.screenshots}\t{k + 1}/10\t\t");
          sw.Restart();
          bool success = model.Run(seed, sample.limit);

          Console.Write($"<{seed}>\t");
          Console.Write($"{sw.ElapsedMilliseconds}ms\t");

          if (success)
          {
            Console.WriteLine("DONE");
            model.Save($"output/{sample.name} {seed}.png");
            if (model is SimpleTiledModel stmodel && ((SimpleTiled)sample).textOutput)
              System.IO.File.WriteAllText($"output/{sample.name} {seed}.txt", stmodel.TextOutput());
            break;
          }
          else
          {
            Console.WriteLine("CONTRADICTION");
            // If we fail, try again with a new seed
            seed = random.Next();
          }
        }
      }
    }

  }
}
