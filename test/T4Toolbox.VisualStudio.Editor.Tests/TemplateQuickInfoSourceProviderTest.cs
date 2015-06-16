﻿// <copyright file="TemplateQuickInfoSourceProviderTest.cs" company="Oleg Sych">
//  Copyright © Oleg Sych. All Rights Reserved.
// </copyright>

namespace T4Toolbox.VisualStudio.Editor
{
    using System;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Linq;
    using Microsoft.VisualStudio.Language.Intellisense;
    using Microsoft.VisualStudio.Utilities;
    using NSubstitute;
    using Xunit;

    public class TemplateQuickInfoSourceProviderTest
    {
        [Fact]
        public void TemplateQuickInfoSourceProviderIsInternalAndNotMeantForPublicConsumption()
        {
            Assert.False(typeof(TemplateQuickInfoSourceProvider).IsPublic);
        }

        [Fact]
        public void TemplateQuickInfoSourceProviderIsSealedAndNotMeantToHaveChildClasses()
        {
            Assert.True(typeof(TemplateQuickInfoSourceProvider).IsSealed);
        }

        [Fact]
        public void TemplateQuickInfoSourceProviderImplementsIQuickSourceProviderInterface()
        {
            Assert.Equal(typeof(IQuickInfoSourceProvider), typeof(TemplateQuickInfoSourceProvider).GetInterfaces()[0]);
        }

        [Fact]
        public void TemplateQuickInfoSourceProviderExportsIQuickSourceProviderInterface()
        {
            ExportAttribute attribute = typeof(TemplateQuickInfoSourceProvider).GetCustomAttributes(false).OfType<ExportAttribute>().Single();
            Assert.Equal(typeof(IQuickInfoSourceProvider), attribute.ContractType);
        }

        [Fact]
        public void TemplateQuickInfoSourceProviderSpecifiesTextTemplateContentType()
        {
            ContentTypeAttribute attribute = typeof(TemplateQuickInfoSourceProvider).GetCustomAttributes(false).OfType<ContentTypeAttribute>().Single();
            Assert.Equal(TemplateContentType.Name, attribute.ContentTypes);
        }

        [Fact]
        public void TemplateQuickInfoSourceProviderSpecifiesNameAttributeRequiredByVisualStudio()
        {
            Assert.Equal(1, typeof(TemplateQuickInfoSourceProvider).GetCustomAttributes(false).OfType<NameAttribute>().Count());
        }

        [Fact]
        public void TemplateQuickInfoSourceProviderSpecifiesOrderAttributeRequiredByVisualStudio()
        {
            Assert.Equal(1, typeof(TemplateQuickInfoSourceProvider).GetCustomAttributes(false).OfType<OrderAttribute>().Count());
        }

        [Fact]
        public void TemplateQuickInfoSourceProviderCanBeConstructedByVisualStudio()
        {
            var catalog = new TypeCatalog(
                typeof(TemplateQuickInfoSourceProvider),
                typeof(SubstituteExporter<ITemplateEditorOptions>));
            var container = new CompositionContainer(catalog);

            Lazy<IQuickInfoSourceProvider> export = container.GetExport<IQuickInfoSourceProvider>();
            Assert.IsType<TemplateQuickInfoSourceProvider>(export.Value);
        }

        [Fact]
        public void ConstructorThrowsArgumentNullExceptionWhenOptionsIsNullToFailFast()
        {
            var e = Assert.Throws<ArgumentNullException>(() => new TemplateQuickInfoSourceProvider(null));
            Assert.Equal("options", e.ParamName);
        }

        [Fact]
        public void CreateTaggerThrowsArgumentNullExceptionWhenBufferIsNullToFailFast()
        {
            var provider = new TemplateQuickInfoSourceProvider(Substitute.For<ITemplateEditorOptions>());
            var e = Assert.Throws<ArgumentNullException>(() => provider.TryCreateQuickInfoSource(null));
            Assert.Equal("buffer", e.ParamName);
        }

        [Fact]
        public void TryCreateQuickInfoSourceReturnsTemplateQuickInfoSource()
        {
            ITemplateEditorOptions options = OptionsWithQuickInfoTooltipsEnabled(true);
            var provider = new TemplateQuickInfoSourceProvider(options);
            var textBuffer = new FakeTextBuffer(string.Empty);
            IQuickInfoSource quickInfoSource = provider.TryCreateQuickInfoSource(textBuffer);
            Assert.Equal(typeof(TemplateQuickInfoSource), quickInfoSource.GetType());
        }

        [Fact]
        public void TryCreateQuickInfoSourceReturnsSameObjectWhenCalledMultipleTimesForSameBuffer()
        {
            ITemplateEditorOptions options = OptionsWithQuickInfoTooltipsEnabled(true);
            var provider = new TemplateQuickInfoSourceProvider(options);
            var textBuffer = new FakeTextBuffer(string.Empty);
            IQuickInfoSource source1 = provider.TryCreateQuickInfoSource(textBuffer);
            IQuickInfoSource source2 = provider.TryCreateQuickInfoSource(textBuffer);
            Assert.Same(source1, source2);
        }

        [Fact]
        public void TryCreateQuickInfoSourceReturnsNullWhenQuickInfoTooltipsAreDisabled()
        {
            ITemplateEditorOptions options = OptionsWithQuickInfoTooltipsEnabled(false);
            var provider = new TemplateQuickInfoSourceProvider(options);
            Assert.Null(provider.TryCreateQuickInfoSource(new FakeTextBuffer(string.Empty)));
        }

        private static ITemplateEditorOptions OptionsWithQuickInfoTooltipsEnabled(bool enabled)
        {
            var options = Substitute.For<ITemplateEditorOptions>();
            options.QuickInfoTooltipsEnabled.Returns(enabled);
            return options;
        }
    }
}