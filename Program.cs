// Copyright (C) 2016 Maxim Gumin, The MIT License (MIT)

using System;
using System.Diagnostics;
using Schema;
using System.Xml.Serialization;
using System.Collections.Generic;

#nullable enable

static class Program
{
  static (int, List<SimpleTiled>, List<Overlapping>) loadSamples(string[] args)
  {
    List<SimpleTiled> simpleTiled = new();
    List<Overlapping> overlapping = new();

    // Split args into two lists, ones starting with ! go into black list, others go into white list
    List<string> whiteList = new();
    List<string> blackList = new();
    foreach (string arg in args)
    {
      if (arg.StartsWith("!"))
        blackList.Add(arg.Substring(1));
      else
        whiteList.Add(arg);
    }

    XmlSerializer serializer = new XmlSerializer(typeof(Samples));
    Samples? samples = (Samples?)serializer.Deserialize(System.IO.File.OpenRead("samples.xml"));

    if (samples == null)
    {
      Console.WriteLine("Failed to load samples.xml");
      Environment.Exit(1);
    }

    foreach (SampleBase sample in samples.Items)
    {
      if (whiteList.Count > 0 && !whiteList.Contains(sample.name)) continue;
      if (blackList.Contains(sample.name)) continue;
      if (sample is Overlapping)
      {
        Overlapping o = (Overlapping)sample;
        overlapping.Add(o);
      }
      else if (sample is SimpleTiled)
      {
        SimpleTiled s = (SimpleTiled)sample;
        simpleTiled.Add(s);
      }
    }

    return (samples.Items.Length, simpleTiled, overlapping);
  }
  static Model? CreateModel(SampleBase sample)
  {
    if (sample is Overlapping)
    {
      Overlapping o = (Overlapping)sample;
      return new OverlappingModel(o.name, o.size, o.size, o.periodicInput, o.periodic, o.symmetry, o.ground, o.heuristic);
    }
    else if (sample is SimpleTiled)
    {
      SimpleTiled s = (SimpleTiled)sample;
      return new SimpleTiledModel(s.name, s.subset, s.size, s.size, s.periodic, s.blackBackground, s.heuristic);
    }
    else
    {
      return null;
    }
  }
  static void RunSample(SampleBase sample)
  {
    Console.WriteLine($"==========================================================");
    Console.WriteLine($"({sample.GetType().Name}) {sample.name}\t");
    Stopwatch sw = Stopwatch.StartNew();
    Model? model = CreateModel(sample);
    Console.WriteLine($"> created in {sw.ElapsedMilliseconds}ms");

    if (model == null)
    {
      Console.WriteLine("Unknown (Skipping)");
      return;
    }
    sw.Restart();

    // Use the same seed for each sample
    Random random = new(42);
    for (int i = 0; i < sample.screenshots; i++)
    {
      // Update the seed for each screenshot
      int seed = random.Next();
      for (int attempt = 0; attempt < 10; attempt++)
      {
        Console.Write($"> Run {i + 1}/{sample.screenshots}\t{attempt + 1}/10\t\t");
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
  static void Main(string[] names)
  {
    CleanOutput();

    (int totalSampleCount, List<SimpleTiled> simpleTiled, List<Overlapping> overlapping) = loadSamples(names);


    if (totalSampleCount == 0 || (simpleTiled.Count == 0 && overlapping.Count == 0))
    {
      Console.WriteLine("No samples to run.");
      return;
    }

    Console.WriteLine($"Found {totalSampleCount} samples. Will run {simpleTiled.Count} SimpleTiled, {overlapping.Count} Overlapping");

    foreach (SimpleTiled sample in simpleTiled)
    {
      RunSample(sample);
    }

    foreach (Overlapping sample in overlapping)
    {
      RunSample(sample);
    }
  }

  private static void CleanOutput()
  {
    var folder = System.IO.Directory.CreateDirectory("output");

    foreach (var file in folder.GetFiles()) file.Delete();
  }
}
