using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Linq;
using System.Timers;
using System.Web;

namespace EtkBlazorApp.Services
{
    public class NewOrdersNotificationService : IDisposable
    {
        public event Action<OrderViewModel> OnNewOrderFound;
        public TimeSpan RefreshTime
        {
            get => _refreshTime;
            set
            {
                if (_refreshTime != value && (value.TotalSeconds > 5))
                {
                    _refreshTime = value;
                    _refreshTimer.Interval = value.TotalMilliseconds;
                }
            }
        }

        TimeSpan _refreshTime = TimeSpan.FromSeconds(60);
        Timer _refreshTimer;
        OrderEntity _lastOrder = null;
        bool _isFirstCheck = true;
        readonly IOrderStorage _database;

        public NewOrdersNotificationService(IOrderStorage database)
        {
            _refreshTimer = new Timer();
            _refreshTimer.Elapsed += RefreshTimer_Elapsed;
            _refreshTimer.Interval = _refreshTime.TotalMilliseconds;
            _refreshTimer.Start();
            _database = database;
            RefreshTimer_Elapsed(null, null);
        }

        private async void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var newlastOrder = (await _database.GetLastOrders(1)).FirstOrDefault();

            if (_isFirstCheck || _lastOrder == null)
            {
                _isFirstCheck = false;
                _lastOrder = newlastOrder;
                return;
            }

            if (_lastOrder.order_id != newlastOrder.order_id)
            {
                _lastOrder = newlastOrder;

                var orderVM = new OrderViewModel()
                {
                    City = _lastOrder.payment_city,
                    Customer = HttpUtility.HtmlDecode(_lastOrder.payment_firstname),
                    DateTime = _lastOrder.date_added,
                    OrderId = _lastOrder.order_id.ToString(),
                    TotalPrice = _lastOrder.total,
                    OrderStatus = _lastOrder.order_status
                };


                OnNewOrderFound?.Invoke(orderVM);
            }

        }

        public void Dispose()
        {
            _refreshTimer.Elapsed -= RefreshTimer_Elapsed;
            _refreshTimer.Stop();
            _refreshTimer.Dispose();
        }
    }
}
