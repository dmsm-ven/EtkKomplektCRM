using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace EtkBlazorApp
{
    public class ProductDiscountViewModel : ProductViewModel, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private decimal newPriceInRub;
        public decimal NewPriceInRub
        {
            get => newPriceInRub;
            set
            {
                if (newPriceInRub != value && Price != 0)
                {
                    newPriceInRub = value;
                    discountPercent = CalculateDiscountPercent("RUB");
                    RaisePropertyChanged();
                }
            }
        }

        private decimal newPriceInCurrency;
        public decimal NewPriceInCurrency
        {
            get => newPriceInCurrency;
            set
            {
                if (newPriceInCurrency != value && BasePrice != 0)
                {
                    newPriceInCurrency = value;
                    discountPercent = CalculateDiscountPercent("RUB");
                    RaisePropertyChanged();
                }
            }
        }

        public DateTime DiscountStartDate { get; set; } = DateTime.Now.Date;
        public DateTime DiscountEndDate { get; set; }
        public bool IsExpired => DaysLeft == 0;

        public decimal PriceDiff
        {
            get
            {
                if(BasePriceCurrency == "RUB" && NewPriceInRub != 0 && Price != 0)
                {
                    return NewPriceInRub - Price;
                }
                else if(NewPriceInCurrency != 0 && BasePrice != 0)
                {
                    return NewPriceInCurrency - BasePrice;
                }

                return 0;
            }
        }

        public int DaysLeft
        {
            get
            {
                if(DiscountEndDate > DateTime.Now.Date)
                {
                    return (int)Math.Floor((DiscountEndDate - DateTime.Now.Date).TotalDays);
                }

                return 0;
            }
        }

        int discountPercent;
        public new int DiscountPercent
        {
            get => discountPercent;
            set
            {
                if (discountPercent != value)
                {
                    discountPercent = value;

                    newPriceInRub = (int)((base.Price * (100 - discountPercent)) / 100);
                    newPriceInCurrency = (int)((base.BasePrice * (100 - discountPercent)) / 100);

                    RaisePropertyChanged();

                }
            }
        }

        public ProductDiscountViewModel()
        {
            var olddate = DateTime.Now.AddMonths(1);

            DiscountEndDate = new DateTime(olddate.Year, olddate.Month, 1, 0, 0, 0, olddate.Kind);
        }

        public double PriceInRubDiscountPercent
        {
            get
            {
                if (Price == decimal.Zero)
                {
                    return 0;
                }
                return (double)(1 - NewPriceInRub / Price);
            }
        }
        public double PriceInCurrencyDiscountPercent
        {
            get
            {
                if (BasePrice == decimal.Zero)
                {
                    return 0;
                }
                return (double)(1 - NewPriceInCurrency / BasePrice);
            }
        }

        public bool IsValidDiscount
        {
            get
            {
                return Id != 0 && (newPriceInCurrency != 0 || newPriceInRub != 0) && DiscountEndDate >= DateTime.Now.Date;
            }
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RefreshDiscountPercent()
        {
            discountPercent = CalculateDiscountPercent(BasePriceCurrency);
            RaisePropertyChanged();
        }

        private int CalculateDiscountPercent(string currencyCode)
        {
            if(currencyCode == "RUB" && Price != 0)
            {
                return  (int)((1m - (NewPriceInRub / (decimal)Price)) * 100);
            }
            else if(BasePrice != 0)
            {
                return  (int)((1m - (NewPriceInCurrency / (decimal)BasePrice)) * 100);
            }
            return 0;
        }
    }
}
