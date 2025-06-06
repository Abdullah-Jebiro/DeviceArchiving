import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DeviceListComponent } from './components/device-list/device-list.component';
import { DeviceFormComponent } from './components/device-form/device-form.component';
import { DeviceDetailsComponent } from './components/device-details/device-details.component';

const routes: Routes = [
  { path: '', component: DeviceListComponent },
  { path: 'add', component: DeviceFormComponent },
  { path: 'edit/:id', component: DeviceFormComponent },

];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class DevicesRoutingModule {}
