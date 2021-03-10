using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace App.Inspection
{
    internal class RegistryCollector : CSharpSyntaxWalker
    {
        private readonly Registry _registry;
        private readonly SemanticModel _model;

        public RegistryCollector(Registry registry, SemanticModel model)
        {
            _registry = registry;
            _model = model;
        }

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            var symbol = _model.GetSymbolInfo(node.Name).Symbol;

            if (symbol is INamespaceSymbol ns)
            {
                _registry.AddUsingDirective(node, ns);
            }
        }

        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            var symbol = _model.GetSymbolInfo(node).Symbol;

            if (symbol is IMethodSymbol ctor)
            {
                _registry.AddObjectCreation(node, ctor);   
            }
        }

        public override void VisitImplicitObjectCreationExpression(ImplicitObjectCreationExpressionSyntax node)
        {
            var symbol = _model.GetSymbolInfo(node).Symbol;
            
            base.VisitImplicitObjectCreationExpression(node);
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var symbol = _model.GetSymbolInfo(node).Symbol;

            if (symbol is IMethodSymbol method)
            {
                var invoker = ResolveInvokingTypeSymbol(node);
                
                _registry.AddMethodInvocation(node, method, invoker);
            }
        }
        
        private INamedTypeSymbol? ResolveInvokingTypeSymbol(InvocationExpressionSyntax node)
        {
            var declaration = node.Ancestors()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault();
            
            if (declaration != null)
            {
                var symbol = _model.GetDeclaredSymbol(declaration);

                if (symbol != null)
                {
                    return symbol;
                }
            }

            return null;
        }
    }
}
