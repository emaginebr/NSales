export default interface InvoiceFeeInfo {
    feeId: number;
    invoiceId: number;
    networkId?: number;
    userId?: number;
    amount: number;
    paidAt: string;
}