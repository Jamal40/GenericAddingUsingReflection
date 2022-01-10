using GenericAdding.Data.Context;
using GenericAdding.DTOs;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GenericAdding.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenericAddingController : ControllerBase
    {
        private readonly MainContext _context;

        public GenericAddingController(MainContext context)
        {
            _context = context;
        }

        [HttpPost]
        public ActionResult AddItem(ItemToAdd itemToAdd)
        {
            var tableClassName = itemToAdd.TableName.Singularize().Pascalize();

            //To Get the result DbSet as an example (_context.Products)
            var tableType = Type.GetType($"GenericAdding.Data.Models.{tableClassName}, GenericAdding");
            if (tableType is null)
            {
                return BadRequest("This table doesn't exist");
            }

            var contextType = typeof(MainContext);
            var setMethod = contextType.GetMethod("Set", Array.Empty<Type>());
            var genericSetMethod = setMethod?.MakeGenericMethod(tableType);
            var resultList = genericSetMethod?.Invoke(_context, null);

            //To Get the Add Method as an example (_context.Products.Add) => without calling it 
            var dbSetType = typeof(DbSet<>);
            var dbSetGenericType = dbSetType.MakeGenericType(new[] {tableType});
            var addMethod = dbSetGenericType.GetMethod("Add");

            //Getting the entity to add ready as an example (var entityToAdd = new Product{Name= "", InstallationFees=500})
            var entityToAdd = Activator.CreateInstance(tableType);
            try
            {
                itemToAdd.Fields.ForEach(f =>
                {
                    var prop = tableType.GetProperty(f.Name.Pascalize());
                    if (prop is null)
                    {
                        throw new Exception($"The field {f.Name} doesn't exist");
                    }

                    var fieldType = prop.PropertyType;
                    var fieldConvertedValue = Convert.ChangeType(f.Value, fieldType);
                    prop.SetValue(entityToAdd, fieldConvertedValue, null);
                });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            //Calling Add method
            addMethod?.Invoke(resultList, new[] {entityToAdd});

            //Calling Save Changes Method
            var saveChangesMethod = contextType.GetMethod("SaveChanges", Array.Empty<Type>());
            saveChangesMethod?.Invoke(_context, null);

            //Or Simply
            _context.SaveChanges();

            return Ok();
        }
    }
}