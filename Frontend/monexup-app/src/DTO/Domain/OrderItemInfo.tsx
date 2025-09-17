import ProductInfo from "./ProductInfo";

export default interface OrderItemInfo {
    itemId: number;
    orderId: number;
    productId: number;
    quantity: number;
    product: ProductInfo;
}