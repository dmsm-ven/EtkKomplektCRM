using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Web;

namespace EtkBlazorApp.Services
{
    public class NewOrdersNotificationService
    {
        readonly Timer timer;
        readonly IOrderStorage orderStorage;

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
                if(timer.Enabled && onNewOrderFound == null)
                {
                    timer.Stop();
                }
            }
        }
      
        TimeSpan refreshInterval = TimeSpan.FromSeconds(30);
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

        OrderEntity _lastOrder = null;
        bool _isFirstCheck = true;

        public NewOrdersNotificationService(IOrderStorage orderStorage)
        {
            timer = new Timer();
            timer.Elapsed += RefreshTimer_Elapsed;
            timer.Enabled = false;
            this.orderStorage = orderStorage;
        }

        private async void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if(onNewOrderFound == null) 
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
}
