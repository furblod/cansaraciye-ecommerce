function increaseQuantity(productId) {
    let input = document.getElementById("quantity-" + productId);
    let value = parseInt(input.value) || 1;
    input.value = value + 1;
}

// Miktarı azaltma işlemi
function decreaseQuantity(productId) {
    let input = document.getElementById("quantity-" + productId);
    let value = parseInt(input.value) || 1;
    if (value > 1) {
        input.value = value - 1;
    }
}

// Sepete ekleme işlemi (Adet kadar ekleyecek)
function addToCart(productId) {
    let quantity = document.getElementById("quantity-" + productId).value;

    // Kullanıcının yanlış bir değer girmesini engelle
    if (quantity < 1 || isNaN(quantity)) {
        quantity = 1;
    }

    // URL'e yönlendirme işlemi (Belirtilen adet kadar eklenecek)
    window.location.href = `/ShoppingCart/AddToCart?productId=${productId}&quantity=${quantity}`;
}
// Sepet miktarını güncelleme
function updateCartQuantity(cartItemId, change) {
    let input = document.getElementById("cart-quantity-" + cartItemId);
    let newQuantity = parseInt(input.value) || 1;

    // Eğer kullanıcı + veya - butonuna basmışsa değiştir
    if (change !== 0) {
        newQuantity += change;
    }

    // 1'in altına düşmesini engelle
    if (newQuantity < 1) {
        newQuantity = 1;
    }

    // Güncellenen değeri input'a yaz
    input.value = newQuantity;

    // AJAX ile backend'e güncellenmiş miktarı gönder
    fetch(`/ShoppingCart/UpdateQuantity?cartItemId=${cartItemId}&quantity=${newQuantity}`, {
        method: 'POST'
    }).then(response => {
        if (response.ok) {
            window.location.reload(); // Sayfayı yenileyerek güncellenmiş miktarı göster
        }
    });
}


document.addEventListener("DOMContentLoaded", function () {
    let stockWarning = document.querySelector(".stock-warning");
    let checkoutButton = document.getElementById("checkoutButton");

    if (stockWarning) {
        checkoutButton.disabled = true;
    } else {
        checkoutButton.disabled = false;
    }
});
