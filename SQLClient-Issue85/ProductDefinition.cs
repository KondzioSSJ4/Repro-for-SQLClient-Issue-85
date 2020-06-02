using SQLClient_Issue85.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SQLClient_Issue85
{
    public class ProductDefinition : BulkCopyTableDefinition<Products>
    {
        public ProductDefinition()
        {
            this.TableName = "Products";
            this.Schema = new List<ObjectDefinition<Products>>()
            {
                //GetObjectValue(p => p.ProductId, "ProductId"),
                GetObjectValue(p => p.ProductName,"ProductName" )        ,
                GetObjectValue(p => p.SupplierId, "SupplierID")              ,
                GetObjectValue(p => p.CategoryId, "CategoryID")              ,
                GetObjectValue(p => p.QuantityPerUnit, "QuantityPerUnit")    ,
                GetObjectValue(p => p.UnitPrice, "UnitPrice")                ,
                GetObjectValue(p => p.UnitsInStock, "UnitsInStock")          ,
                GetObjectValue(p => p.UnitsOnOrder, "UnitsOnOrder")          ,
                GetObjectValue(p => p.ReorderLevel, "ReorderLevel")          ,
                GetObjectValue(p => p.Discontinued, "Discontinued")
            };
        }

        
    }
}
