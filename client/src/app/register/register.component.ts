import { Component, inject, input, output, } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  private accountService = inject(AccountService);
  private toastr = inject(ToastrService);
  private router = inject(Router);
  //userFromHomeComponent = input.required<any>();
  cancelRegister = output<boolean>();
  model: any = {};

  register() {
    this.accountService.register(this.model).subscribe({
      next: _ => this.router.navigateByUrl('/members'),
      error: output => this.toastr.error(output.error)
    });
  }

  cancel() {
    this.cancelRegister.emit(false);
  }
}
