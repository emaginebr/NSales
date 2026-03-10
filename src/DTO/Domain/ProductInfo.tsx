import { ProductStatusEnum } from "../Enum/ProductStatusEnum";

export default interface ProductInfo {
    productId: number;
    networkId: number;
    slug: string;
    image: string;
    imageUrl: string;
    name: string;
    description: string;
    price: number;
    frequency: number;
    limit: number;
    status: ProductStatusEnum;
}