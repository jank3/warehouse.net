﻿using System;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Warehouse.Silverlight.Data.Interfaces;
using Warehouse.Silverlight.Infrastructure;
using Warehouse.Silverlight.Infrastructure.Events;
using Warehouse.Silverlight.Models;

namespace Warehouse.Silverlight.MainModule
{
    public class ProductCreateWindowViewModel : InteractionRequestValidationObject
    {
        private readonly IProductsRepository repository;
        private readonly IEventAggregator eventAggregator;

        private string id;
        private string name;
        private string size;
        private string k;
        private string priceOpt;
        private long priceRozn;
        private double weight;
        private string count;
        private string nd;
        private string length;

        private bool isSheet;
        private double[] sheetSizes;
        private bool isBusy;

        public ProductCreateWindowViewModel(IProductsRepository repository, IEventAggregator eventAggregator)
        {
            this.repository = repository;
            this.eventAggregator = eventAggregator;

            SaveCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(() => IsWindowOpen = false);
        }

        public ProductCreateWindowViewModel Init()
        {
            var product = new Product();
            ProductToProps(product);
            IsWindowOpen = true;
            return this;
        }

        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        public string Title2
        {
            get
            {
                var label = isSheet ? " (лист)" : string.Empty;
                return string.Format("Новая позиция{0}", label);
            }
        }

        public bool IsBusy
        {
            get { return isBusy; }
            set { isBusy = value; RaisePropertyChanged(() => IsBusy); }
        }

        #region Name

        public string Name
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    name = value;
                    ValidateName();
                }
            }
        }

        private void ValidateName()
        {
            errorsContainer.ClearErrors(() => Name);
            errorsContainer.SetErrors(() => Name, Validate.Required(Name));
        }

        #endregion

        #region Size

        public string Size
        {
            get { return size; }
            set
            {
                if (size != value)
                {
                    size = value;
                    ValidateSize();
                    if (isSheet)
                    {
                        UpdateSheetLength();
                    }
                }
            }
        }

        private void ValidateSize()
        {
            errorsContainer.ClearErrors(() => Size);
            errorsContainer.SetErrors(() => Size, Validate.Required(Size));
            if (isSheet)
            {
                sheetSizes = ParseSheetSize(size);
                if (sheetSizes == null)
                {
                    errorsContainer.SetErrors(() => Size, new[] { "строка в формате\nтолщина*ширина*длина" });
                }
            }
        }

        #endregion

        #region K

        public string K
        {
            get { return k; }
            set
            {
                if (k != value)
                {
                    k = value;
                    ValidateK();
                    UpdatePriceRozn();
                    UpdateWeight();
                }
            }
        }

        private void ValidateK()
        {
            errorsContainer.ClearErrors(() => K);
            errorsContainer.SetErrors(() => K, Validate.Double(K));
        }

        #endregion

        #region PriceOpt

        public string PriceOpt
        {
            get { return priceOpt; }
            set
            {
                if (priceOpt != value)
                {
                    priceOpt = value;
                    ValidatePriceOpt();
                    UpdatePriceRozn();
                }
            }
        }

        private void ValidatePriceOpt()
        {
            errorsContainer.ClearErrors(() => PriceOpt);
            errorsContainer.SetErrors(() => PriceOpt, Validate.Long(PriceOpt));
        }

        #endregion

        #region PriceRozn

        public long PriceRozn
        {
            get { return priceRozn; }
            set
            {
                if (priceRozn != value)
                {
                    priceRozn = value;
                    RaisePropertyChanged(() => PriceRozn);
                }
            }
        }

        private void UpdatePriceRozn()
        {
            if (errorsContainer.HasErrors(() => PriceOpt, () => K, () => Length))
            {
                PriceRozn = 0;
            }
            else
            {
                var _priceOpt = decimal.Parse(priceOpt);
                var _k = decimal.Parse(k);
                var rozn = _priceOpt * _k / 1000m * 1.2m;
                if (isSheet)
                {
                    var _l = decimal.Parse(length);
                    rozn *= _l;
                }
                PriceRozn = (long) (decimal.Ceiling(rozn / 100) * 100);
            }
        }

        #endregion

        #region Weight

        public double Weight
        {
            get { return weight; }
            set
            {
                if (Math.Abs(weight - value) > double.Epsilon)
                {
                    weight = value;
                    RaisePropertyChanged(() => Weight);
                }
            }
        }

        private void UpdateWeight()
        {
            if (errorsContainer.HasErrors(() => Count, () => Length, () => K, () => Nd))
            {
                Weight = 0;
            }
            else
            {
                var _count = int.Parse(count);
                var _length = double.Parse(length);
                var _nd = GetTotalNd();
                var _k = double.Parse(k);

                Weight = Math.Round( (_count * _length + _nd) * _k, 3);
            }
        }

        #endregion

        #region Count

        public string Count
        {
            get { return count; }
            set
            {
                if (count != value)
                {
                    count = value;
                    ValidateCount();
                    UpdateWeight();
                }
            }
        }

        private void ValidateCount()
        {
            errorsContainer.ClearErrors(() => Count);
            errorsContainer.SetErrors(() => Count, Validate.Int(Count));
        }

        #endregion

        #region Nd

        public string Nd
        {
            get { return nd; }
            set
            {
                if (nd != value)
                {
                    nd = value;
                    ValidateNd();
                    UpdateWeight();
                }
            }
        }

        private void ValidateNd()
        {
            errorsContainer.ClearErrors(() => Nd);
            if (!string.IsNullOrEmpty(nd))
            {
                var parts = nd.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var x in parts)
                {
                    var errors = Validate.Double(x).ToArray();
                    if (errors.Length > 0)
                    {
                        errorsContainer.SetErrors(() => Nd, new[] { "дробные числа, разделенные пробелом" });
                        break;
                    }
                }
            }
        }

        public string NdLabel
        {
            get { return isSheet ? "Н/Д (м²)" : "Н/Д (м)"; }
        }

        #endregion

        #region Length

        public string LenghtLabel
        {
            get { return isSheet ? "Площадь листа (м²)" : "Длина штанги (м)"; }
        }

        public string Length
        {
            get { return length; }
            set
            {
                if (length != value)
                {
                    length = value;
                    ValidateLength();
                    UpdateWeight();
                    RaisePropertyChanged(() => Length);
                }
            }
        }

        private void ValidateLength()
        {
            errorsContainer.ClearErrors(() => Length);
            errorsContainer.SetErrors(() => Length, Validate.Double(Length));
        }

        private void UpdateSheetLength()
        {
            if (errorsContainer.HasErrors(() => Size) || sheetSizes == null)
            {
                Length = "0";
            }
            else
            {
                var val = sheetSizes[1] / 1000 * sheetSizes[2] / 1000;
                Length = val.ToString("0.000");
            }
        }

        #endregion

        #region PriceIcome
        private string priceIcome;

        public string PriceIcome
        {
            get { return priceIcome; }
            set
            {
                if (priceIcome != value)
                {
                    priceIcome = value;
                    ValidatePriceIcome();
                }
            }
        }

        private void ValidatePriceIcome()
        {
            errorsContainer.ClearErrors(() => PriceIcome);
            errorsContainer.SetErrors(() => PriceIcome, Validate.Long(PriceIcome));
        }

        #endregion

        #region Internal

        public string Internal { get; set; }

        #endregion

        #region IsSheet

        public bool IsSheet
        {
            get { return isSheet; }
            set
            {
                if (isSheet != value)
                {
                    isSheet = value;
                    ValidateSize();
                    UpdateSheetLength();
                    UpdatePriceRozn();
                    RaisePropertyChanged(() => IsSheet);
                    RaisePropertyChanged(() => Title2);
                    RaisePropertyChanged(() => LenghtLabel);
                    RaisePropertyChanged(() => NdLabel);
                }
            }
        }

        #endregion

        #region Firma

        public string Firma { get; set; }

        #endregion

        private async void Save()
        {
            ValidateName();
            ValidateSize();
            ValidateK();
            ValidatePriceOpt();
            ValidateCount();
            ValidateNd();
            ValidateLength();
            ValidatePriceIcome();

            if (HasErrors) return;

            var changed = PropsToProduct();

            IsBusy = true;
            var task = await repository.SaveAsync(changed);
            IsBusy = false;
            if (task.Succeed)
            {
                var args = new ProductUpdatedEventArgs(task.Result, false);
                eventAggregator.GetEvent<ProductUpdatedEvent>().Publish(args);
                Confirmed = true;
                IsWindowOpen = false;
            }
        }

        private void ProductToProps(Product product)
        {
            id = product.Id;
            name = product.Name;
            size = product.Size;
            k = product.K.ToString("0.##");
            priceOpt = product.PriceOpt.ToString(CultureInfo.InvariantCulture);
            priceRozn = product.PriceRozn;
            weight = product.Weight;
            count = product.Count.ToString(CultureInfo.InvariantCulture);
            if (product.Nd != null)
            {
                nd = string.Join(" ", product.Nd);
            }
            length = product.Length.ToString("0.##");
            priceIcome = product.PriceIcome.ToString(CultureInfo.InvariantCulture);
            Internal = product.Internal;
            IsSheet = product.IsSheet;
            Firma = product.Firma;
        }

        private Product PropsToProduct()
        {
            return new Product
            {
                Id = id,
                Name = name,
                Size = size,
                K = Math.Round(double.Parse(k), 2),
                PriceOpt = long.Parse(priceOpt),
                PriceRozn = priceRozn,
                Weight = weight,
                Count = int.Parse(count),
                Nd = ParseNd(nd),
                Length = Math.Round(double.Parse(length), 2),
                PriceIcome = long.Parse(priceIcome),
                Internal = Internal,
                IsSheet = isSheet,
                Firma = Firma,
            };
        }

        private static double[] ParseNd(string nd)
        {
            return (nd ?? string.Empty)
                .Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                .Select(double.Parse)
                .OrderByDescending(x => x)
                .ToArray();
        }

        private double GetTotalNd()
        {
            if (string.IsNullOrEmpty(nd))
            {
                return 0;
            }
            return nd.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).Select(double.Parse).Sum();
        }

        private static double[] ParseSheetSize(string size)
        {
            if (size == null) return null;

            var parts = size.Split(new[] { ' ', '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return null;

            var sizes = parts[0].Split(new[] { '*' }, StringSplitOptions.RemoveEmptyEntries);
            if (sizes.Length != 3) return null;

            double d;
            if (sizes.All(x => double.TryParse(x, out d)))
            {
                return sizes.Select(double.Parse).ToArray();
            }

            return null;
        }
    }
}