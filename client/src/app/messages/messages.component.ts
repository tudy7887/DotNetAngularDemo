import { Component, inject, OnInit } from '@angular/core';
import { MessageService } from '../_services/message.service';

@Component({
  selector: 'app-messages',
  standalone: true,
  imports: [],
  templateUrl: './messages.component.html',
  styleUrl: './messages.component.css'
})
export class MessagesComponent implements OnInit {
  messageService = inject(MessageService);
  continer = 'Inbox';
  pageNumber = 1;
  pageSize = 5;

  ngOnInit(): void {
    this.loadMessages();
  }

  loadMessages(){
    this.messageService.getMesages(this.pageNumber, this.pageSize, this.continer)
  }

  pageChanged(event: any) {
    if (this.pageNumber !== event.page) {
      this.pageNumber = event.page;
      this.loadMessages();
    }
  }
}
