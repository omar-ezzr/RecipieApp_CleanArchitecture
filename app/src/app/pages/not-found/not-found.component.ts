import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
@Component({ standalone: true, imports: [RouterLink], template: `<section class="page-shell status-page"><p class="ui-label">404</p><h1 class="display-title">Page not found</h1><p class="body-copy">The page you're looking for doesn't exist or may have moved.</p><div><a class="btn btn-primary-app" routerLink="/recipes">Back to recipes</a><a class="btn btn-secondary-app" routerLink="/feed">Go home</a></div></section>`, styles: [`.status-page{display:grid;gap:var(--space-4);max-width:760px;padding-block:var(--space-8)}.status-page p{margin:0}.status-page div{display:flex;flex-wrap:wrap;gap:var(--space-3)}`] })
export class NotFoundComponent {}
