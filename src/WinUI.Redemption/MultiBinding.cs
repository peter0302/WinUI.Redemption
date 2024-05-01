using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Markup;

using WinUI.Redemption.Internal;

namespace WinUI.Redemption
{
    /// <summary>
    /// Implements a WPF/MAUI-style multi-binding framework. Note currently
    /// only one-way bindings (from source to target) are supported.
    /// </summary>
    [ContentProperty(Name = nameof(Bindings))]
    public class MultiBinding
    {
        /// <summary>
        /// An attached <see cref="DependencyProperty"/> for assigning one or more
        /// <see cref="MultiBinding"/>s to a <see cref="FrameworkElement"/> via a
        /// <see cref="MultiBindingCollection"/> in XAML. When this is used in a control or
        /// data template, the best practice is to declare a <see cref="MultiBindingCollection"/> 
        /// as a resource and assign it to this attached property through
        /// <c>StaticResource</c>, to reduce the amount of reflection needed
        /// to resolve target property names.
        /// </summary>
        public static readonly DependencyProperty MultiBindingsProperty = DependencyProperty.RegisterAttached(
            "MultiBindings",
            typeof(MultiBindingCollection),
            typeof(MultiBinding),
            new PropertyMetadata(null, (sender, e) =>
            {
                if (!(sender is FrameworkElement fe) ||
                    !(e.NewValue is MultiBindingCollection mbc))
                    return;
                foreach (var mb in mbc)
                {
                    if (string.IsNullOrEmpty(mb.PropertyName))
                        throw new InvalidOperationException(
                            $"{nameof(mb.PropertyName)} required when instantiating " +
                            $"a {nameof(MultiBinding)} from XAML.");

                    var dp = mb._resolvedProperty ?? (mb._resolvedProperty = 
                        (mb.PropertyOwner ?? fe.GetType()).TryGetDependencyProperty(mb.PropertyName));

                    if (dp == null)
                        throw new InvalidOperationException(
                            $"Could not locate {mb.PropertyName}Property on {fe.GetType()}; " +
                            $"did you forget to specify {nameof(mb.PropertyOwner)}?");

                    Bind(fe, dp, mb);
                }
            }));
        public static MultiBindingCollection GetMultiBindings(DependencyObject obj)
        {
            return obj.GetValue(MultiBindingsProperty) as MultiBindingCollection;
        }
        public static void SetMultiBindings(DependencyObject obj, MultiBindingCollection value)
        {
            obj.SetValue(MultiBindingsProperty, value);
        }

        /// <summary>
        /// Used to create a <see cref="MultiBinding"/> binding through code.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <param name="targetProperty">The target property.</param>
        /// <param name="multiBinding">A <see cref="MultiBinding"/>
        /// instance. Note the <see cref="MultiBinding.Property"/>
        /// and related properties are unneeded as the 
        /// <paramref name="targetProperty"/> will always be used.
        /// </param>
        public static void Bind(
            FrameworkElement target, 
            DependencyProperty targetProperty,
            MultiBinding multiBinding)
        {
            multiBinding.Validate();
            var mbx = new MultiBindingExpression
            {
                MultiBinding = multiBinding,
                Target = target,
                TargetProperty = targetProperty,
            };
            mbx.Apply();
        }

        /// <summary>
        /// A collection of <see cref="MultiBindingSource"/> instances.
        /// This is only set directly when applying
        /// a <see cref="MultiBinding"/> in code using
        /// <see cref="MultiBinding.Bind(FrameworkElement, DependencyProperty, MultiBinding)"/>,
        /// otherwise in XAML the <see cref="MultiBindingSource"/>s can just
        /// be added as content to the <see cref="MultiBinding"/>.
        /// </summary>
        public Collection<MultiBindingSource> Bindings
        {
            get => _Bindings ?? (_Bindings = new Collection<MultiBindingSource>());
            set => _Bindings = value;
        }

        /// <summary>
        /// Specifies the name of the target <see cref="DependencyProperty"/>.
        /// This must be populated when a <see cref="MultiBinding"/> is declared in XAML 
        /// as a member of a <see cref="MultiBindingCollection"/>. 
        /// <see cref="PropertyOwner"/> is also required when targeting attached properties 
        /// or if the <see cref="DependencyProperty"/> isn't actually a member of the
        /// target type or its ancestors for some other reason.
        /// Presumably there are more efficient ways of doing this that 
        /// WinUI doesn't make available to the muggles, so alas we are
        /// relegated to using reflection. Accordingly, in templates, 
        /// a <see cref="MultiBindingCollection"/> should be declared as a resource and
        /// assigned to the target's <see cref="MultiBinding.MultiBindingsProperty"/>
        /// through <c>StaticResource</c>, which results in having to resolve the
        /// target <see cref="DependencyProperty"/> only once per template.
        /// </summary>
        public string PropertyName 
        { 
            get;
            set;
        }

        /// <summary>
        /// In connection with <see cref="PropertyName"/>, used to
        /// unambiguously identify the owner of the target
        /// <see cref="DependencyProperty"/>. Required for 
        /// attached properties since <see cref="MultiBinding"/> does not have 
        /// access to XAML namespaces and so can't resolve the property class,
        /// or if the <see cref="DependencyProperty"/> instance isn't actually 
        /// a member of the target type or its ancestors for some other reason.
        /// </summary>
        public Type PropertyOwner
        {
            get;
            set;
        }

        /// <summary>
        /// A required <see cref="IMultiValueConverter"/> instance 
        /// which receives the multiple values resulting from the
        /// <see cref="MultiBindingSource"/>s and converts them to
        /// a final target value for the target property.
        /// </summary>
        public IMultiValueConverter Converter
        {
            get;
            set;
        }

        /// <summary>
        /// An optional value that is passed along to
        /// <see cref="Converter"/> whenever any 
        /// <see cref="MultiBindingSource"/> value changes.
        /// </summary>
        public object ConverterParameter
        {
            get;
            set;
        }

        private class ProxyCount
        {
            public int Count 
            { 
                get; 
                set; 
            }
        }

        private void Validate()
        {
            if (_Bindings?.Any() != true)
                throw new InvalidOperationException($"At least one {nameof(MultiBindingSource)} is required.");
            if (this.Converter == null)
                throw new InvalidOperationException($"A {nameof(IMultiValueConverter)} is required.");
            foreach (var binding in _Bindings)
            {
                if (binding.Binding.Mode != BindingMode.OneWay)
                    throw new InvalidOperationException(
                        $"Only {nameof(BindingMode.OneWay)} bindings are currently supported.");
            }
        }

        private Collection<MultiBindingSource> _Bindings;
        private DependencyProperty _resolvedProperty;

        private class MultiBindingProxy
        {
            public Binding OriginalBinding
            {
                get;
                init;
            }

            public DependencyProperty ProxyProperty
            {
                get;
                init;
            }

            public int Index
            {
                get;
                init;
            }
        }

        private class MultiBindingExpression : INotifyPropertyChanged
        {
            object _FinalValue;
            [Obfuscation(Exclude = true)]
            public object FinalValue
            {
                get
                {
                    return _FinalValue;
                }
                set
                {
                    if (_FinalValue == value)
                        return;
                    _FinalValue = value;
                    this.PropertyChanged?.Invoke(
                        this, 
                        new PropertyChangedEventArgs(nameof(FinalValue)));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            internal MultiBinding MultiBinding { get; init; }

            internal FrameworkElement Target { get; set; }

            internal DependencyProperty TargetProperty { get; init; }

            internal MultiBindingProxy[] Proxies { get; set; }

            internal object[] ProxyValues { get; private set; }

            internal void Apply()
            {
                _isApplying = true;

                if (!_proxiesUsed.TryGetValue(this.Target, out var count))
                {
                    count = new ProxyCount();
                    _proxiesUsed.Add(this.Target, count);
                }

                this.Proxies = new MultiBindingProxy[this.MultiBinding.Bindings.Count];
                this.ProxyValues = new object[this.MultiBinding.Bindings.Count];

                for (int i = 0; i < MultiBinding.Bindings.Count; i++)
                {
                    var binding = MultiBinding.Bindings.ElementAt(i).Binding;
                    var proxy = new MultiBindingProxy
                    {
                        OriginalBinding = binding,
                        ProxyProperty = GetProxyProperty(count.Count++),
                        Index = i,
                    };                    
                    BindingOperations.SetBinding(
                        this.Target,
                        proxy.ProxyProperty,
                        new Binding
                        {
                            Source = binding.Source,
                            ElementName = binding.ElementName,
                            FallbackValue = binding.FallbackValue,
                            Mode = BindingMode.OneWay,
                            Path = binding.Path,
                            RelativeSource = binding.RelativeSource,
                            TargetNullValue = binding.TargetNullValue,
                            UpdateSourceTrigger = binding.UpdateSourceTrigger,                            
                            ConverterLanguage = binding.ConverterLanguage,

                            // We'll use the originals from binding later in our own converter
                            Converter = new MultiBindingProxyConverter(this, proxy),
                            ConverterParameter = null
                        });
                    this.Proxies[i] = proxy;
                }

                _isApplying = false;
                Reevaluate();
                BindingOperations.SetBinding(
                    this.Target,
                    this.TargetProperty,
                    new Binding
                    {
                        Source = this,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                        Path = new PropertyPath(nameof(FinalValue)),
                        Mode = BindingMode.OneWay,
                    });

                // This not only ensures we don't needlessly evaluate until the
                // target is loaded but also ensures the expression isn't GC'd until
                // after the FE is unloaded. (Assuming bindings in WinUI are weak refs,
                // which I don't actually know, but anyway...).
                this.Target.Loaded += this.OnTargetLoaded;
                this.Target.Unloaded += this.OnTargetUnloaded;
            }

            internal void Reevaluate()
            {
                if (_isApplying || this.Target?.IsLoaded != true)
                    return;
                this.FinalValue = this.MultiBinding.Converter.Convert(
                    this.ProxyValues,
                    this.MultiBinding.ConverterParameter);
            }

            private void OnTargetLoaded(object sender, RoutedEventArgs e)
            {
                this.Reevaluate();
            }

            private void OnTargetUnloaded(object sender, RoutedEventArgs e)
            {
                this.Unapply();
            }

            private void Unapply()
            {
                // There's no need to do anything else because all the
                // bindings will just go away on their own.
                this.Target.Loaded -= this.OnTargetLoaded;
                this.Target.Unloaded -= this.OnTargetUnloaded;
                this.Target = null;
            }            

            private static DependencyProperty GetProxyProperty(int i)
            {
                // Each MultiBindingSource uses an attached "proxy" property on the target
                // to listen for changes to the source. As long as the same target
                // instance doesn't use the proxy property more than once, 
                // all of the properties are fungible as we don't actually do anything
                // with the values; the converter does all the work. This is a big
                // improvement from my original implementation for XF which 
                // created new properties for every MulitBinding, which
                // had to do all the work of resolving the binding sources.
                // Now the total number of properties needed by the entire application
                // will never be larger than the largest # of multibinding sources any one
                // FE instance uses, so it should never grow too big in practice.
                // The other big advantage is that by attaching the proxy property
                // directly to the target of the multibinding, we get data context, relative
                // sources, etc. all for free.
                lock (_proxyProps)
                {
                    var dp = _proxyProps[i];
                    if (dp != null)
                        return dp;
                    dp = DependencyProperty.Register(
                        $"MBProxy{i}",
                        typeof(object),
                        typeof(MultiBinding),
                        new PropertyMetadata(null));
                    return _proxyProps[i] = dp;
                }
            }

            private static readonly GrowableArray<DependencyProperty> _proxyProps = 
                new GrowableArray<DependencyProperty>(10); // not thread safe
            private static ConditionalWeakTable<FrameworkElement, ProxyCount> _proxiesUsed =
                new ConditionalWeakTable<FrameworkElement, ProxyCount>();   // already thread safe

            private bool _isApplying;
        }

        private class MultiBindingProxyConverter : IValueConverter
        {
            MultiBindingExpression _expression;
            MultiBindingProxy _proxy;

            public MultiBindingProxyConverter(
                MultiBindingExpression expression,
                MultiBindingProxy proxy)
            {
                _proxy = proxy;
                _expression = expression;
            }

            // This will be called any time this source's value changes
            public object Convert(object value, Type targetType, object parameter, string language)
            {                
                if (_proxy.OriginalBinding.Converter != null)
                {
                    // Now use the original binding's converter and parameter
                    // if there was one
                    value = _proxy.OriginalBinding.Converter.Convert(
                        value,
                        targetType,
                        _proxy.OriginalBinding.ConverterParameter,
                        language);
                }

                _expression.ProxyValues[_proxy.Index] = value;
                _expression.Reevaluate();

                return value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, string language)
            {
                // TODO: TwoWay support
                throw new NotImplementedException();
            }
        }
    }
}
