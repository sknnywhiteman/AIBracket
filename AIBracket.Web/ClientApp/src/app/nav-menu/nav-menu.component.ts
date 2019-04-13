import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { ProfileResponseModel } from '../models/ProfileResponseModel';
import { isEmpty } from 'rxjs/operators';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {

  @Input('Account')
  account: ProfileResponseModel;
  @Output() logout: EventEmitter<boolean> = new EventEmitter<boolean>();
  @Output() sidenav: EventEmitter<boolean> = new EventEmitter<boolean>();

  constructor(private router: Router) { }

  viewProfile() {
    this.router.navigate(['/profile']);
  }

  getDisplayName() {
    if (this.account.firstName == '' && this.account.lastName == '') {
      return this.account.userName;
    }
    else {
      return this.account.firstName + ' ' + this.account.lastName;
    }
  }

  Logout() {
    this.logout.emit(true);
    this.router.navigate(['/home']);
  }

  OpenNav() {
    this.sidenav.emit(true);
  }
}
