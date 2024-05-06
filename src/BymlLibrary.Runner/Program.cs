#if RELEASE
using BenchmarkDotNet.Running;
using BymlLibrary.Runner.Benchmarks;

BenchmarkRunner.Run<BymlBenchmarks>();
return;
#else

using BymlLibrary;
using Revrs;

string yaml = File.ReadAllText(args[0]);
Byml fromYaml = Byml.FromText(yaml);

byte[] toBinary = fromYaml.ToBinary(Endianness.Little, version: 2);
Byml fromBinary = Byml.FromBinary(toBinary);
File.WriteAllBytes(args[1], toBinary);

string fromBinaryToYaml = fromBinary.ToYaml();
File.WriteAllText(args[2], fromBinaryToYaml);

#endif