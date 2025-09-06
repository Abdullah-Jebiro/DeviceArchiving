import { CreateDeviceDto } from "./create-device.model";


export interface UpdateDeviceDto extends CreateDeviceDto {

}

export interface AuthenticationResponse {
    token: string;
    userName: string;
    picture: string;
}


export interface BaseResponse<T> {
  success: boolean;
  data: T;
  message: string;
}

