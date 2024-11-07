import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-roles-modal',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './roles-modal.component.html',
  styleUrl: './roles-modal.component.css'
})
export class RolesModalComponent {
  bsModalRef = inject(BsModalRef);
  username = '';
  title = '';
  availableRoles: string[] = [];
  selectedeRoles: string[] = [];
  rolesUpdated = false;

  onSelectedRoles() {
    this.rolesUpdated =  true;
    this.bsModalRef.hide();
  }

  updateChecked(checkedValue: string) {
    if(this.selectedeRoles.includes(checkedValue)){
      this.selectedeRoles = this.selectedeRoles.filter(r => r !== checkedValue);
    } else {
      this.selectedeRoles.push(checkedValue);
    }
  }
}
