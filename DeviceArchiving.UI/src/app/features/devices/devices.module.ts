import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { DevicesRoutingModule } from './devices-routing.module';
import { DeviceListComponent } from './components/device-list/device-list.component';
import { DeviceFormComponent } from './components/device-form/device-form.component';
import { DeviceDetailsComponent } from './components/device-details/device-details.component';
import { SharedModule } from '../../shared/shared.module';
import { FileUploadModule } from 'primeng/fileupload';
import { DeletedDevicesComponent } from './components/deleted-devices/deleted-devices.component';

@NgModule({
  declarations: [
    DeviceListComponent,
    DeviceFormComponent,
    DeviceDetailsComponent,
    DeletedDevicesComponent
  ],
  imports: [
    DevicesRoutingModule,
    CommonModule,
    FileUploadModule,
    SharedModule
  ]
})
export class DevicesModule {}