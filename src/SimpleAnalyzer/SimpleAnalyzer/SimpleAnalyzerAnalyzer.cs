﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text.Json;

namespace SimpleAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class SimpleAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";
        private const string HtmlHelpUri = "https://github.com/timheuer/SimpleAnalyzer";
        private List<Term> terms;
        private static readonly DiagnosticDescriptor WarningRule = new DiagnosticDescriptor("TERM001", Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description, helpLinkUri: HtmlHelpUri);
        private static readonly DiagnosticDescriptor ErrorRule = new DiagnosticDescriptor("TERM002", Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description, helpLinkUri: HtmlHelpUri);
        private static readonly DiagnosticDescriptor InfoRule = new DiagnosticDescriptor("TERM003", Title, MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: Description, helpLinkUri: HtmlHelpUri);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(WarningRule, ErrorRule, InfoRule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction((ctx) =>
            {
                if (terms is null)
                {
                    var currentDirecory = GetFolderTypeWasLoadedFrom<SimpleAnalyzerAnalyzer>();
                    terms = JsonSerializer.Deserialize<List<Term>>(File.ReadAllBytes(Path.Combine(currentDirecory, "terms.json")));
                }

                ctx.RegisterSymbolAction((symbolContext) =>
                {
                    var symbol = symbolContext.Symbol;

                    foreach (var term in terms)
                    {
                        if (ContainsUnsafeWords(symbol.Name, term.Name))
                        {
                            var diag = Diagnostic.Create(GetRule(term, symbol.Name), symbol.Locations[0], term.Name, symbol.Name, term.Severity, term.Class);
                            symbolContext.ReportDiagnostic(diag);
                            break;
                        }
                    }

                }, SymbolKind.NamedType, SymbolKind.Method, SymbolKind.Property, SymbolKind.Field,
                        SymbolKind.Event, SymbolKind.Namespace, SymbolKind.Parameter);
            });
        }

        private static string GetFolderTypeWasLoadedFrom<T>() => new FileInfo(new Uri(typeof(T).Assembly.CodeBase).LocalPath).Directory.FullName;

        private DiagnosticDescriptor GetRule(Term term, string identifier)
        {
            var warningLevel = DiagnosticSeverity.Info;
            var diagId = "TERM001";
            var description = $"Recommendation: {term.Recommendation}{System.Environment.NewLine}Context: {term.Context}{System.Environment.NewLine}Reason: {term.Why}{System.Environment.NewLine}Term ID: {term.Id}";
            switch (term.Severity)
            {
                case "1":
                case "2":
                    warningLevel = DiagnosticSeverity.Error;
                    diagId = "TERM002";
                    break;
                case "3":
                    warningLevel = DiagnosticSeverity.Warning;
                    break;
                default:
                    warningLevel = DiagnosticSeverity.Info;
                    diagId = "TERM003";
                    break;
            }

            return new DiagnosticDescriptor(diagId, Title, MessageFormat, term.Class, warningLevel, isEnabledByDefault: true, description: description, helpLinkUri: HtmlHelpUri, term.Name);
        }

        private bool ContainsUnsafeWords(string symbol, string term)
        {
            return term.Length < 4 ?
                symbol.Equals(term, StringComparison.InvariantCultureIgnoreCase) :
                symbol.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }
    }
}
