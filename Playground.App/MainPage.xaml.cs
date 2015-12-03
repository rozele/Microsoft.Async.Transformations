using Microsoft.Async.Transformations;
using Microsoft.Async.Transformations.Windows;
using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

using static Microsoft.Async.Transformations.AsyncTransform;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Playground.App
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly DisposableAsyncFunction _resetAsync;
        private readonly DisposableAsyncFunction _restartAsync;
        private readonly DisposableAsyncFunction _startStopAsync;

        public MainPage()
        {
            InitializeComponent();
            DataContext = Context;

            var resetCoreAsync = Identity(Context.ResetAsync);
            var restartCoreAsync = Identity(Context.RestartAsync).Switch();
            var startStopCoreAsync = Identity(Context.RunAsync).Toggle();
            var asyncFuncList = SwitchMany(resetCoreAsync, restartCoreAsync.InvokeAsync, startStopCoreAsync.InvokeAsync);

            _resetAsync = asyncFuncList[0];
            _restartAsync = new DisposableAsyncFunction(asyncFuncList[1].InvokeAsync, new CompositeDisposable(asyncFuncList[1], restartCoreAsync));
            _startStopAsync = new DisposableAsyncFunction(asyncFuncList[2].InvokeAsync, new CompositeDisposable(asyncFuncList[2], startStopCoreAsync));
        }

        public StopwatchContext Context { get; } = new StopwatchContext();

        private async void RestartButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await _restartAsync.InvokeAsync(CancellationToken.None);
        }

        private async void StartStopToggleSwitch_Toggled(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await _startStopAsync.InvokeAsync(CancellationToken.None);
        }

        private async void ResetButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await _resetAsync.InvokeAsync(CancellationToken.None);
        }
    }
}
