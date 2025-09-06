import { Component } from '@angular/core';
import { AccountService } from './core/services/account.service';
import { UserRole } from './core/models/authentication.model';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: false,
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'DeviceArchiving.UI';

  ngOnInit(): void {

  }
}
