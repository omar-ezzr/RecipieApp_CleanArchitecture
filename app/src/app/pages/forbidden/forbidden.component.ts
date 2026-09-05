import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
@Component({ standalone: true, imports: [RouterLink], template: `<section class="page-shell status-page"><p class="ui-label">403</p><h1 class="display-title">Access restricted</h1><p class="body-copy">You don't have permission to access this page.</p><a class="btn btn-primary-app" routerLink="/recipes">Back to recipes</a></section>`, styles: [`.status-page{display:grid;gap:var(--space-4);max-width:760px;padding-block:var(--space-8)}.status-page p{margin:0}`] })
export class ForbiddenComponent {}
