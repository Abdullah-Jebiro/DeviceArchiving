import { Component, OnInit, OnDestroy } from '@angular/core';
import { MessageService } from 'primeng/api';
import { DeviceService } from '../../../../core/services/device.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DevicesDto } from '../../../../core/models/device.model';

@Component({
  selector: 'app-deleted-devices',
  templateUrl: './deleted-devices.component.html',
  styleUrls: ['./deleted-devices.component.css'],
  standalone: false,
  providers: [MessageService]
})
export class DeletedDevicesComponent implements OnInit, OnDestroy {
  deletedDevices: DevicesDto[] = [];
  filteredDevices: DevicesDto[] = [];
  deviceTypes: { label: string, value: string }[] = [];
  selectedDevice: DevicesDto | null = null;
  displayConfirmDialog: boolean = false;
  loading: boolean = false;
  globalSearchQuery: string = '';
  searchCriteria = {
    laptopName: '',
    serialNumber: '',
    type: ''
  };

  private destroy$ = new Subject<void>();

  columnHeaders: { [key: string]: string } = {
    source: 'الجهة',
    brotherName: 'اسم الأخ',
    laptopName: 'اسم اللاب توب',
    systemPassword: 'كلمة مرور النظام',
    windowsPassword: 'كلمة مرور ويندوز',
    hardDrivePassword: 'كلمة التشفير',
    freezePassword: 'كلمة التجميد',
    code: 'الكود',
    type: 'النوع',
    serialNumber: 'الرقم التسلسلي',
    card: 'الكرت',
    comment: 'ملاحظات',
    contactNumber: 'رقم التواصل',
    userName: 'تم بواسطة',
    createdAt: 'تاريخ الإنشاء',
    actions: 'الإجراءات'
  };

  displayedColumns: string[] = [
    'source', 'brotherName', 'laptopName', 'systemPassword', 'windowsPassword',
    'hardDrivePassword', 'freezePassword', 'code', 'type', 'serialNumber', 'card',
    'comment', 'contactNumber', 'userName', 'createdAt', 'actions'
  ];

  constructor(
    private deviceService: DeviceService,
    private messageService: MessageService
  ) { }

  ngOnInit(): void {
    this.loadDeletedDevices();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadDeletedDevices(): void {
    this.loading = true;
    this.deviceService.getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (devices: DevicesDto[]) => {
          this.deletedDevices = devices.filter(d => d.isActive === false);
          this.filteredDevices = [...this.deletedDevices];
          this.deviceTypes = [...new Set(this.deletedDevices.map(d => d.type))]
            .filter(Boolean)
            .map(type => ({ label: type!, value: type! }));
          this.loading = false;
        },
        error: () => {
          this.loading = false;
          this.messageService.add({
            severity: 'error',
            summary: 'خطأ',
            detail: 'فشل تحميل الأجهزة.'
          });
        }
      });
  }

  applyFilter(): void {
    let filtered = [...this.deletedDevices];

    if (this.globalSearchQuery) {
      const q = this.globalSearchQuery.toLowerCase();
      filtered = filtered.filter(d =>
        (d.laptopName?.toLowerCase().includes(q) ?? false) ||
        (d.serialNumber?.toLowerCase().includes(q) ?? false) ||
        (d.type?.toLowerCase().includes(q) ?? false) ||
        (d.source?.toLowerCase().includes(q) ?? false) ||
        (d.brotherName?.toLowerCase().includes(q) ?? false)
      );
    }

    if (this.searchCriteria.laptopName) {
      const q = this.searchCriteria.laptopName.toLowerCase();
      filtered = filtered.filter(d =>
        d.laptopName?.toLowerCase().includes(q) ?? false
      );
    }

    if (this.searchCriteria.serialNumber) {
      const q = this.searchCriteria.serialNumber.toLowerCase();
      filtered = filtered.filter(d =>
        d.serialNumber?.toLowerCase().includes(q) ?? false
      );
    }

    if (this.searchCriteria.type) {
      filtered = filtered.filter(d => d.type === this.searchCriteria.type);
    }

    this.filteredDevices = filtered;
  }

  clearSearch(): void {
    this.globalSearchQuery = '';
    this.searchCriteria = {
      laptopName: '',
      serialNumber: '',
      type: ''
    };
    this.applyFilter();
  }

  showConfirmDialog(device: DevicesDto): void {
    this.selectedDevice = device;
    this.displayConfirmDialog = true;
  }

  selectDevice(device: DevicesDto): void {
    this.selectedDevice = device;
  }

  async confirmRestore(): Promise<void> {
    if (!this.selectedDevice) return;

    try {
      await this.deviceService.restoreDevice(this.selectedDevice.id).toPromise();
      this.deletedDevices = this.deletedDevices.filter(d => d.id !== this.selectedDevice!.id);
      this.filteredDevices = this.filteredDevices.filter(d => d.id !== this.selectedDevice!.id);
      this.messageService.add({
        severity: 'success',
        summary: 'نجاح',
        detail: 'تم استعادة الجهاز بنجاح',
        life: 5000
      });
    } catch (error: any) {
      this.messageService.add({
        severity: 'error',
        summary: 'خطأ',
        detail: `خطأ أثناء استعادة الجهاز: ${error?.message || 'غير معروف'}`,
        life: 5000
      });
    } finally {
      this.displayConfirmDialog = false;
      this.selectedDevice = null;
    }
  }

  cancelRestore(): void {
    this.displayConfirmDialog = false;
    this.selectedDevice = null;
  }
}