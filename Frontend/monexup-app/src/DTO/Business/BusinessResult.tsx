import StatusRequest from "../Services/StatusRequest";

interface BusinessResult<T> extends StatusRequest {
  dataResult: T
}

export default BusinessResult;