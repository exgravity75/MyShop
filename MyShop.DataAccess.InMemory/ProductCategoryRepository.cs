using MyShop.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.DataAccess.InMemory
{
    public class ProductCategoryRepository
    {
        ObjectCache cache = MemoryCache.Default;
        List<ProductCategory> productCategories = new List<ProductCategory>();

        public ProductCategoryRepository()
        {
            productCategories = cache["productCategories"] as List<ProductCategory>;
            if (productCategories == null)
                productCategories = new List<ProductCategory>();
        }

        public void Commit()
        {
            cache["productCategories"] = productCategories;
        }

        public void Insert(ProductCategory productCategory)
        {
            productCategories.Add(productCategory);
        }

        public void Update(ProductCategory productCategory)
        {
            ProductCategory toUpdate = productCategories.Find(p => p.Id == productCategory.Id);

            if (toUpdate != null)
            {
                toUpdate = productCategory;
            }
            else
            {
                throw new Exception("ProductCategory Not Found");
            }
        }

        public ProductCategory Find(string Id)
        {
            ProductCategory product = productCategories.Find(p => p.Id == Id);
            if (product != null)
            {
                return product;
            }
            else
            {
                throw new Exception("ProductCategory Not Found");
            }
        }

        public IQueryable<ProductCategory> Collection()
        {
            return productCategories.AsQueryable();
        }

        public void Delete(string Id)
        {
            ProductCategory toDelete = productCategories.Find(p => p.Id == Id);

            if (toDelete != null)
            {
                productCategories.Remove(toDelete);
            }
            else
            {
                throw new Exception("ProductCategory Not Found");
            }
        }
    }
}
