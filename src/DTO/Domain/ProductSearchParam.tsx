export default interface ProductSearchParam {
    networkId: number;
    userId: number;
    networkSlug: string;
    userSlug: string;
    keyword: string;
    onlyActive: boolean;
    pageNum: number;
}