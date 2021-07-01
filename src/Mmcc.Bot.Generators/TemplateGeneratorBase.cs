using System.Linq;
using Microsoft.CodeAnalysis;

namespace Mmcc.Bot.Generators
{
    /// <summary>
    /// Represents a base class to be inherited by template generators.
    /// </summary>
    public abstract class TemplateGeneratorBase
    {
        /// <summary>
        /// Whitespace representing one indentation level.
        /// </summary>
        private const string Indentation = "    ";
        
        /// <summary>
        /// Annotates a type with <code>global::</code>.
        /// </summary>
        /// <param name="type">Type to be annotated.</param>
        /// <returns>The type annotated with <code>global::</code></returns>
        protected string AnnotateTypeWithGlobal(string type) => $"global::{type}";

        /// <summary>
        /// Annotates a type with <code>global::</code>.
        /// </summary>
        /// <param name="type">Type to be annotated.</param>
        /// <returns>The type annotated with <code>global::</code></returns>
        protected string AnnotateTypeWithGlobal(INamedTypeSymbol type) => $"global::{type}";
        
        /// <summary>
        /// Indents a <see cref="string"/>.
        /// </summary>
        /// <param name="s">String to be indented.</param>
        /// <param name="indentLevel">Indentation level</param>
        /// <returns>
        /// String <paramref name="s"/> indented with the indentation level <paramref name="indentLevel"/>
        /// </returns>
        protected string Indent(string s, int indentLevel) =>
            $"{string.Concat(Enumerable.Repeat(Indentation, indentLevel))}{s}";

        /// <summary>
        /// Generates the source code from the template.
        /// </summary>
        /// <returns>Generated source code.</returns>
        public string Generate() =>
            FillInStub(GenerateFiller());

        /// <summary>
        /// Fills in the stub with the generated filler.
        /// </summary>
        /// <param name="generatedFiller">The generated filler.</param>
        /// <returns>The stub filled in with the <paramref name="generatedFiller"/>.</returns>
        protected abstract string FillInStub(string generatedFiller);

        /// <summary>
        /// Generates the filler to be filled into the stub.
        /// </summary>
        /// <returns>The generated filler.</returns>
        protected abstract string GenerateFiller();
    }
}