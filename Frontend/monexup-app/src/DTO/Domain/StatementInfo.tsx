export default interface StatementInfo {
    invoiceId: number;
    feeId: number;
    networkId: number;
    networkName: string;
    userId: number;
    buyerName: string;
    sellerId: number;
    sellerName: string;
    description: string;
    paymentDate: string;
    amount: number;
    paidAt: string;
}