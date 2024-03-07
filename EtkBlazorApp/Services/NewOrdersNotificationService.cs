using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Runtime.CompilerServices;
using System.Timers;

namespace EtkBlazorApp.Services;

/// <summary>
/// Сервис оповещения UI о новых заказаз, проваерка по таймеру
/// </summary>
public class NewOrdersNotificationService
{
    private readonly Timer timer;
    private readonly IOrderStorage orderStorage;

    private event Action<OrderEntity> onNewOrderFound;
    public event Action<OrderEntity> OnNewOrderFound
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        add
        {
            onNewOrderFound = (Action<OrderEntity>)Delegate.Combine(onNewOrderFound, value);
            if (!timer.Enabled && onNewOrderFound != null)
            {
                timer.Start();
                RefreshTimer_Elapsed(null, null);
            }
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        remove
        {
            onNewOrderFound = (Action<OrderEntity>)Delegate.Remove(onNewOrderFound, value);
            if (timer.Enabled && onNewOrderFound == null)
            {
                timer.Stop();
            }
        }
    }

    private TimeSpan refreshInterval = TimeSpan.FromSeconds(30);
    public TimeSpan RefreshInterval
    {
        get => refreshInterval;
        set
        {
            if (value.TotalSeconds < 5)
            {
                throw new ArgumentOutOfRangeException("Минимальный период обновление = 5 секунд");
            }
            if (refreshInterval != value)
            {
                refreshInterval = value;
                timer.Interval = value.TotalMilliseconds;
            }
        }
    }

    private OrderEntity _lastOrder = null;
    private bool _isFirstCheck = true;

    public NewOrdersNotificationService(IOrderStorage orderStorage)
    {
        timer = new Timer();
        this.orderStorage = orderStorage ?? throw new ArgumentNullException(nameof(orderStorage));
    }

    public void Start()
    {
        timer.Elapsed += RefreshTimer_Elapsed;
        timer.Enabled = false;
    }

    private async void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        if (onNewOrderFound == null)
        {
            return;
        }

        var currentLastOrder = await orderStorage.GetLastOrder();

        if (_isFirstCheck || _lastOrder == null)
        {
            _isFirstCheck = false;
            _lastOrder = currentLastOrder;
            return;
        }

        if (_lastOrder.order_id != currentLastOrder.order_id)
        {
            _lastOrder = currentLastOrder;

            onNewOrderFound?.Invoke(_lastOrder);
        }

    }
}
