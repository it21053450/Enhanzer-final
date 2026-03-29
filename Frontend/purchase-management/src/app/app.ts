/**
 * @file app.ts
 * @description Root application component.
 * Provides the top navigation bar and router outlet.
 */

import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  /** Application name displayed in the navigation bar */
  readonly appTitle = 'Purchase Management';
}
