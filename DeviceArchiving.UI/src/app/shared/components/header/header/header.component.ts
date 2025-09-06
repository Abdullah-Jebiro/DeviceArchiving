import { AccountService } from '../../../../core/services/account.service';
import { Router } from '@angular/router';
import { Component, Input, OnInit } from '@angular/core';
import { UserRole } from '../../../../core/models/authentication.model';
interface NavPage {
  label: string;
  route: string;
}

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css'],
  standalone: false,
})
export class HeaderComponent implements OnInit {

  @Input() pages: NavPage[] = [];

  userName: string = 'غير معروف';
  pictureUrl: string = './1.jpg';
  role: string = '';
  constructor(public accountService: AccountService) { }

  ngOnInit(): void {
    this.accountService.userInfo$.subscribe(user => {
      this.userName = user.userName ?? 'غير معروف';
      this.pictureUrl = './1.jpg';
      this.role = user.role ?? '';
      this.pages = [
        { label: 'الصفحة الرئيسة', route: '/devices' },
        { label: 'ادارة العمليات', route: '/operation-types' },
        { label: 'الأجهزة المحذوفة', route: '/devices/delete-devices' },
      ];
      if (user.role === UserRole.Admin) {
        this.pages.push({ label: 'إنشاء حساب', route: '/account/signup' });
      }

    });
  }


  logout(): void {
    this.accountService.logout();
  }
}