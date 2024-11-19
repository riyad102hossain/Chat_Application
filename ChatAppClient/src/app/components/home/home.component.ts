import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { UserModel } from '../../models/user.model';
import { ChatModel } from '../../models/chat.model';
import { HttpClient } from '@angular/common/http';
import * as signalR from '@microsoft/signalr';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';


@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule,FormsModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  users: UserModel[] = [];
  chats: ChatModel[] = [];
  selectedUserId: string = "";
  selectedUser: UserModel = new UserModel();
  user = new UserModel();
  hub: signalR.HubConnection | undefined;
  message: string = "";  
  selectedFile: File | null = null;  // MOD

  constructor(
    private http: HttpClient
  ){
    this.user = JSON.parse(localStorage.getItem("accessToken") ?? "");
    this.getUsers();

    this.hub = new signalR.HubConnectionBuilder().withUrl("http://localhost:5142/chat-hub").build();

    this.hub.start().then(()=> {
      console.log("Connection is started...");  
      
      this.hub?.invoke("Connect", this.user.id);

      this.hub?.on("Users", (res:UserModel) => {
        console.log(res);
        this.users.find(p=> p.id == res.id)!.status = res.status;        
      });

      this.hub?.on("Messages",(res:ChatModel)=> {
        console.log(res);        
        
        if(this.selectedUserId == res.userId){
          this.chats.push(res);
        }
      })
    })
  }

  getUsers(){
    this.http.get<UserModel[]>("http://localhost:5142/api/Chat/GetUsers").subscribe(res=> {
      this.users = res.filter(p => p.id != this.user.id);
    })
  }

  changeUser(user: UserModel){
    this.selectedUserId = user.id;
    this.selectedUser = user;

    this.http.get(`http://localhost:5142/api/Chat/GetChats?userId=${this.user.id}&toUserId=${this.selectedUserId}`).subscribe((res:any)=>{
      this.chats = res;
    });
  }

  logout(){
    localStorage.clear();
    document.location.reload();
  }

   

onFileSelected(event: any): void {
  const file: File = event.target.files[0];

  if (file) {
    // Check if the file is not an image
    if (!this.isImage(file.name)) {
      this.selectedFile = file;
      console.log('File selected: ', this.selectedFile);
    } else {
      alert("Please select only non-image files (e.g., .pdf, .docx, .zip)");
      this.selectedFile = null;
    }
  }
}

sendMessage(): void {
  const formData = new FormData();
 
  formData.append("UserId", this.user.id); 
formData.append("ToUserId", this.selectedUserId); 
formData.append("Message", this.message || ""); 

  // Attach the selected file if there is one
  if (this.selectedFile) {
    formData.append("file", this.selectedFile);
    console.log('File appended to formData: ', this.selectedFile);
  } else {
    console.log('No file selected.');
  }

  // Debugging output for formData
  console.log('Sending FormData:');
  formData.forEach((value, key) => {
    console.log(`${key}:`, value);
  });

  // Make the HTTP POST request
  this.http.post<ChatModel>("http://localhost:5142/api/Chat/SendMessage", formData).subscribe(
    (res) => {
      this.chats.push(res);       // Assuming chats is an array where the new message is added
      this.message = "";          // Clear message input
      this.selectedFile = null;   // Clear selected file after sending
    },
    (error) => {
      console.error("Failed to send message or file", error);
    }
  );
}

// Check if the file is an image by its extension
isImage(filePath: string): boolean {
  const imageExtensions = ['jpg', 'jpeg', 'png', 'gif', 'bmp', 'svg'];
  const extension = filePath.split('.').pop()?.toLowerCase();
  return extension ? imageExtensions.includes(extension) : false;
}
}