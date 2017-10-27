using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CppSharp;
using CppSharp.AST;
using CppSharp.Generators;
using CppSharp.Parser;
using CppSharp.Passes;
using Type = CppSharp.AST.Type;

namespace ServoSharp
{
    class Program
    {
        static readonly string RootPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;

        static void Main(string[] args)
        {
            ConsoleDriver.Run(new ServoLib());
            var source = Path.Combine(RootPath, "ServoSharp.cs");
            var dest = Path.Combine(Directory.GetParent(RootPath).FullName, "Servofocus\\ServoSharp.cs");
            File.Copy(source, dest, overwrite: true);
            Console.ReadKey();
        }

        class ServoLib : ILibrary
        {
            public void Preprocess(Driver driver, ASTContext ctx)
            {
            }

            public void Postprocess(Driver driver, ASTContext ctx)
            {
                var libNamespace = ctx.FindClass("Size").Single().Namespace;

                
                //ctx.SetClassAsValueType("HostCallbacks");
                //ctx.SetClassAsValueType("Size");
                //ctx.SetClassAsValueType("Margins");
                //ctx.SetClassAsValueType("Position");
                //ctx.SetClassAsValueType("ViewLayout");

                var initWithEgl = ctx.FindFunction("InitWithEgl").Single();
                initWithEgl.GenerationKind = GenerationKind.None;

                var performUpdates = ctx.FindFunction("PerformUpdates").Single();
                performUpdates.GenerationKind = GenerationKind.None;

                var loadUrl = ctx.FindFunction("LoadUrl").Single();
                loadUrl.GenerationKind = GenerationKind.None;

                var scroll = ctx.FindFunction("Scroll").Single();
                scroll.GenerationKind = GenerationKind.None;

                var version = ctx.FindFunction("ServoVersion").Single();
                version.GenerationKind = GenerationKind.None;
                
                var servoSharp = new Class
                {
                    Name = "ServoSharp",
                    Access = AccessSpecifier.Public,
                    Type = ClassType.RefType,
                    Namespace = libNamespace,
                };
            
                servoSharp.Methods.AddRange(new[]
                {
                    new Method{ Namespace = servoSharp, IsDefaultConstructor = true, Kind = CXXMethodKind.Constructor },
                    new Method(initWithEgl){ Namespace = servoSharp, GenerationKind = GenerationKind.Generate },
                    new Method(performUpdates){ Namespace = servoSharp, GenerationKind = GenerationKind.Generate},
                    new Method(loadUrl){ Namespace = servoSharp, GenerationKind = GenerationKind.Generate},
                    new Method(scroll){ Namespace = servoSharp, GenerationKind = GenerationKind.Generate},
                    new Method(version){ Namespace = servoSharp, GenerationKind = GenerationKind.Generate}
                });

                ctx.TranslationUnits[0].Classes.Add(servoSharp);

                ctx.FindEnum("ServoResult").Single().Items.ForEach(item => item.Name = item.Name.Replace("ServoResult", string.Empty)); 
                ctx.FindEnum("ScrollState").Single().Items.ForEach(item => item.Name = item.Name.Replace("ScrollState", string.Empty));    
            }

            public void Setup(Driver driver)
            {
                driver.ParserOptions = new ParserOptions
                {
                    IncludeDirs = new List<string> { RootPath },
                    //Verbose = true,
                };

                var options = driver.Options;
                options.GeneratorKind = GeneratorKind.CSharp;
                options.CompileCode = true;
                options.StripLibPrefix = false;
                options.GenerateSingleCSharpFile = true;
                options.OutputDir = RootPath;

                var module = options.AddModule("ServoSharp");
                module.SharedLibraryName = "libservobridge";
                module.IncludeDirs.AddRange(new[] { RootPath });
                module.Headers.Add("libservobridge.h");
                module.OutputNamespace = "Servofocus";
            }

            public void SetupPasses(Driver driver)
            {
                driver.Context.TranslationUnitPasses.AddPass(new FunctionToInstanceMethodPass());
                driver.Context.TranslationUnitPasses.AddPass(new DelegatesPass());
            }
        }
    }
}
