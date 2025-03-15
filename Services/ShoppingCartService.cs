using cansaraciye_ecommerce.Data;
using cansaraciye_ecommerce.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cansaraciye_ecommerce.Services
{
    public class ShoppingCartService
    {
        private readonly ApplicationDbContext _context;

        public ShoppingCartService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Sepete ürün ekleme
        public async Task AddToCartAsync(int productId, string userId)
        {
            var cartItem = await _context.ShoppingCartItems
                .FirstOrDefaultAsync(c => c.ProductId == productId && c.UserId == userId);

            if (cartItem != null)
            {
                cartItem.Quantity++;
            }
            else
            {
                cartItem = new ShoppingCartItem
                {
                    ProductId = productId,
                    UserId = userId,
                    Quantity = 1
                };
                await _context.ShoppingCartItems.AddAsync(cartItem);
            }

            await _context.SaveChangesAsync();
        }

        // Kullanıcının sepetini getir
        public async Task<List<ShoppingCartItem>> GetCartItems(string userId)
        {
            return await _context.ShoppingCartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        // Sepetten ürün silme
        public async Task RemoveFromCartAsync(int cartItemId)
        {
            var cartItem = await _context.ShoppingCartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                _context.ShoppingCartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
        }

        // Sepeti boşalt
        public async Task ClearCartAsync(string userId)
        {
            var cartItems = _context.ShoppingCartItems.Where(c => c.UserId == userId);
            _context.ShoppingCartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
        }

        public async Task IncreaseQuantityAsync(int cartItemId)
        {
            var cartItem = await _context.ShoppingCartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                cartItem.Quantity += 1;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DecreaseQuantityAsync(int cartItemId)
        {
            var cartItem = await _context.ShoppingCartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                if (cartItem.Quantity > 1)
                {
                    cartItem.Quantity -= 1;
                }
                else
                {
                    _context.ShoppingCartItems.Remove(cartItem);
                }
                await _context.SaveChangesAsync();
            }
        }

    }
}
