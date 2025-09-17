declare global {
  interface Window {
    transferBitcoin: (poolAddr: string, amount: number, network: string, callback: any) => void;
  }
}

export { }