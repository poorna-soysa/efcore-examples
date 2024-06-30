// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using EFCoreExample;


BenchmarkRunner.Run<CompliedQueriesBenchmarkExample>();

//QueryTagsExample queryTags = new();

//queryTags.GetTodos();


