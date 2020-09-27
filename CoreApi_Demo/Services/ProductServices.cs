using CoreApi_Demo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreApi_Demo.Services
{   

    public interface IProductService
    {
        IEnumerable<Product> GetAll();
    }

    public class ProductService : IProductService
    {
        public IEnumerable<Product> GetAll()
        {
            var rd = new Random();

            return Enumerable.Range(1, 5).Select(index => new Product()
            {
                Name = $"商品-{index}",
                Price = rd.Next(100, 9999)
            });
        }
    }
}
